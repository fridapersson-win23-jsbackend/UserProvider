using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProvider.Models;

namespace UserProvider.Fucntions;

public class DeleteUser
{
    private readonly ILogger<DeleteUser> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUser(ILogger<DeleteUser> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    [Function("DeleteUser")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "deleteuser/{id}")] HttpRequest req, string id)
    {
        try
        {
            var userToDelete = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
            if(userToDelete != null)
            {
                var result = await _userManager.DeleteAsync(userToDelete);
                if(result.Succeeded)
                {
                    return new OkObjectResult(result);
                }
                return new NotFoundObjectResult(result);
            }
            return new BadRequestResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : DeleteUser.Run() :: {ex.Message} ");
        }
        return null!;
    }
}
