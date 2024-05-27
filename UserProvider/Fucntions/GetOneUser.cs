using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UserProvider.Fucntions
{
    public class GetOneUser
    {
        private readonly ILogger<GetOneUser> _logger;
        private readonly DataContext _context;

        public GetOneUser(ILogger<GetOneUser> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("GetOneUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "getuser/{id}")] HttpRequest req, string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if(user != null)
                {
                    return new OkObjectResult(user);
                }
                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message} ");
            }
            return null!;
        }
    }
}
