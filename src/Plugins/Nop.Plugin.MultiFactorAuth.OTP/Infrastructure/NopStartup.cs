using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.MultiFactorAuth.OTP.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.MultiFactorAuth.OTP.Infrastructure;

/// <summary>
/// Represents plugin startup class
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register HTTP client
        services.AddHttpClient<SmsIrService>();

        // Register services
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ISmsService, SmsIrService>();

        // Register settings
        services.AddScoped(serviceProvider =>
        {
            var settingService = serviceProvider.GetRequiredService<Nop.Services.Configuration.ISettingService>();
            var storeContext = serviceProvider.GetRequiredService<Nop.Core.IStoreContext>();
            var storeScope = storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            return settingService.LoadSettingAsync<OTPSettings>(storeScope).Result;
        });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
        // No specific middleware configuration needed
    }

    /// <summary>
    /// Configure routing
    /// </summary>
    /// <param name="endpoints">Route builder</param>
    public void ConfigureRoutes(IEndpointRouteBuilder endpoints)
    {
        // Admin routes - IMPORTANT: این route باید دقیقاً همان نامی باشد که در OTPDefaults تعریف شده
        endpoints.MapControllerRoute(
            name: OTPDefaults.ConfigurationRouteName,
            pattern: "Admin/OTP/Configure",
            defaults: new { controller = "OTP", action = "Configure", area = "Admin" });

        // Customer routes for OTP operations
        endpoints.MapControllerRoute(
            name: "Customer.SendOtp",
            pattern: "Customer/SendOtp",
            defaults: new { controller = "CustomerOTP", action = "SendOtp" });

        endpoints.MapControllerRoute(
            name: "Customer.VerifyOtp",
            pattern: "Customer/VerifyOtp",
            defaults: new { controller = "CustomerOTP", action = "VerifyOtp" });

        endpoints.MapControllerRoute(
            name: "Customer.EnableOtp",
            pattern: "Customer/EnableOtp",
            defaults: new { controller = "CustomerOTP", action = "EnableOtp" });

        endpoints.MapControllerRoute(
            name: "Customer.DisableOtp",
            pattern: "Customer/DisableOtp",
            defaults: new { controller = "CustomerOTP", action = "DisableOtp" });

        endpoints.MapControllerRoute(
            name: "Customer.GenerateBackupCodes",
            pattern: "Customer/GenerateBackupCodes",
            defaults: new { controller = "CustomerOTP", action = "GenerateBackupCodes" });

        endpoints.MapControllerRoute(
            name: "Customer.GetBackupCodes",
            pattern: "Customer/GetBackupCodes",
            defaults: new { controller = "CustomerOTP", action = "GetBackupCodes" });

        endpoints.MapControllerRoute(
            name: "Customer.VerifyMultiFactorAuth",
            pattern: "Customer/VerifyMultiFactorAuth",
            defaults: new { controller = "CustomerOTP", action = "VerifyMultiFactorAuth" });

        endpoints.MapControllerRoute(
            name: "Customer.ResendOtp",
            pattern: "Customer/ResendOtp",
            defaults: new { controller = "CustomerOTP", action = "ResendOtp" });

        // Route for customer configuration page - this is important for nopCommerce integration
        endpoints.MapControllerRoute(
            name: "CustomerMultiFactorAuthenticationProviderConfig.OTP",
            pattern: "Customer/ConfigureMultiFactorAuthenticationProvider/OTP",
            defaults: new { controller = "CustomerOTP", action = "ConfigureProvider", providerSysName = "MultiFactorAuth.OTP" });
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 11;
}