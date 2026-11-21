using System.Text.Json.Serialization;
using Smurfs.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Smurfs.API.Extensions;
using Smurfs.API.MiddleWare;

namespace Smurfs.API;

/// <summary>
/// Configures the API's service registrations and HTTP request pipeline.
/// </summary>
public class Startup
{
    private readonly IConfiguration _config;

    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Creates the startup component with the application configuration and hosting environment.
    /// </summary>
    public Startup(IConfiguration config, IWebHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    /// <summary>
    /// Registers controller, persistence, and authentication services.
    /// </summary>
    public void ConfigureServices(
        IServiceCollection services)
    {
        services.AddControllers(opt =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            opt.Filters.Add(new AuthorizeFilter(policy));
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<Gender>(allowIntegerValues: false));
        });

        services.AddApplicationServices(_config);
        services.AddIdentityServices(_config, _environment);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// <summary>
    /// Configures middleware and routes for API requests and the hosted WebClient.
    /// </summary>
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        //if (env.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
        //}

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallback("api/{**path}", context =>
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }).AllowAnonymous();
            endpoints.MapFallbackToFile("index.html").AllowAnonymous();
        });
    }
}
