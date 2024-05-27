using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserProvider.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddScoped<UserService>();


        services.AddDbContext<DataContext>(x => x.UseSqlServer(Environment.GetEnvironmentVariable("USER_IDENTITY_DATABASE")));

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

        services.AddScoped<UserManager<ApplicationUser>>();
        services.AddScoped<SignInManager<ApplicationUser>>();
    })
    .Build();

host.Run();
