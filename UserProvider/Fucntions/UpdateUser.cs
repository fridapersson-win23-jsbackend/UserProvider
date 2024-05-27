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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if(requestBody != null)
            {
                var userUpdateReq = JsonConvert.DeserializeObject<UserUpdateModel>(requestBody);

                var user = await _userManager.FindByEmailAsync(userUpdateReq!.Email!);
                if (user != null)
                {
                    user.FirstName = userUpdateReq.FirstName;
                    user.LastName = userUpdateReq.LastName;
                    user.Email = userUpdateReq.Email;
                    user.Biography = userUpdateReq.Biography;
                    user.PhoneNumber = userUpdateReq.PhoneNumber;

                    if (userUpdateReq.Address != null)
                    {
                       var existingAddress = await _context.Addresses.FirstOrDefaultAsync(x =>
                       x.AddressLine_1 == userUpdateReq.Address.AddressLine_1 &&
                       x.AddressLine_2 == userUpdateReq.Address.AddressLine_2 &&
                       x.PostalCode == userUpdateReq.Address.PostalCode &&
                       x.City == userUpdateReq.Address.City);

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
