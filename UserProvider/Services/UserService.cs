using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using UserProvider.Models;

namespace UserProvider.Services;

public class UserService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly DataContext _context;

    public UserService(IServiceScopeFactory scopeFactory, AuthenticationStateProvider authenticationStateProvider, DataContext context)
    {
        _scopeFactory = scopeFactory;
        _authenticationStateProvider = authenticationStateProvider;
        _context = context;
    }

    public async Task<ClaimsPrincipal> GetClaimsAsync()
    {
        try
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            if (authState != null)
            {
                var user = authState.User;

                if (user != null)
                {
                    return user;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR : EmailSender.Run() :: {ex.Message} ");
        }
        return null!;
    }

    //public async Task<ApplicationUser> GetAllUserAsync()
    //{
    //    try
    //    {
    //        using var scope = _scopeFactory.CreateScope();
    //        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    //        //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    //        var users = await context.Users.Include(x => x.Address).ToListAsync();
    //        if(users != null)
    //        {
    //            return users;
    //        }


    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"ERROR: {ex.Message}");
    //    }
    //    return null!;
    //}


    public async Task<bool> UpdateUserAsync(UserUpdateModel userUpdate)
    {
        try
        {
            var userClaims = await GetClaimsAsync();
            if (userClaims != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                var userId = userManager.GetUserId(userClaims);
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.FirstName = userUpdate.FirstName;
                    user.LastName = userUpdate.LastName;
                    user.PhoneNumber = userUpdate.PhoneNumber;
                    user.Biography = userUpdate.Biography;

                    var existingAddress = await dbContext.Addresses
                        .FirstOrDefaultAsync(x => x.AddressLine_1 == userUpdate.Address.AddressLine_1 &&
                                                  x.AddressLine_2 == userUpdate.Address.AddressLine_2 &&
                                                  x.PostalCode == userUpdate.Address.PostalCode &&
                                                  x.City == userUpdate.Address.City);

                    if (existingAddress != null)
                    {
                        user.AddressId = existingAddress.Id;
                    }
                    else
                    {
                        var newAddress = new AddressEntity
                        {
                            AddressLine_1 = userUpdate.Address.AddressLine_1,
                            AddressLine_2 = userUpdate.Address.AddressLine_2,
                            PostalCode = userUpdate.Address.PostalCode,
                            City = userUpdate.Address.City
                        };

                        dbContext.Addresses.Add(newAddress);
                        await dbContext.SaveChangesAsync();

                        user.AddressId = newAddress.Id;
                    }

                    var result = await userManager.UpdateAsync(user);
                    return result.Succeeded;
                }
                else
                {
                    Console.WriteLine("User not found");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: UpdateUserAsync() :: {ex.Message}");
        }
        return false;
    }


}
