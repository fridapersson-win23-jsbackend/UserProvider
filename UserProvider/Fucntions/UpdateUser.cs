using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProvider.Models;

namespace UserProvider.Fucntions;

public class UpdateUser
{
    private readonly ILogger<UpdateUser> _logger;
    private readonly DataContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUser(ILogger<UpdateUser> logger, DataContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [Function("UpdateUser")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "updateuser/{id}")] HttpRequest req, string id)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if(requestBody != null)
            {
                var data = JsonConvert.DeserializeObject<UserUpdateModel>(requestBody);

                var user = await _userManager.FindByEmailAsync(data!.Email!);
                if (user != null)
                {
                    user.FirstName = data.FirstName;
                    user.LastName = data.LastName;
                    user.Email = data.Email;
                    user.Biography = data.Biography;
                    user.PhoneNumber = data.PhoneNumber;

                    if (data.Address != null)
                    {
                       var existingAddress = await _context.Addresses.FirstOrDefaultAsync(x =>
                       x.AddressLine_1 == data.Address.AddressLine_1 &&
                       x.AddressLine_2 == data.Address.AddressLine_2 &&
                       x.PostalCode == data.Address.PostalCode &&
                       x.City == data.Address.City);

                        if(existingAddress != null)
                        {
                            user.Address = existingAddress;
                            user.AddressId = existingAddress.Id;
                        }
                    }
                }
                else
                {
                    return new NotFoundResult();
                }
                var result = await _userManager.UpdateAsync(user);

                if (result != null)
                {
                    return new OkObjectResult(result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message} ");
            return new BadRequestObjectResult(ex);
        }
        return null!;
    }


}
