using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CHC.Consent.Web.UI
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddRazorPagesOptions(
                    setup =>
                    {
                        setup.RootDirectory = "/Pages";
                    });

            services.AddResponseCompression();

            services.AddTransient<CookieAuthenticationEventsHandler>();

            services.AddAuthentication(
                    sharedOptions =>
                    {
                        sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        
                    })
                .AddCookie(
                    _ =>
                    {
                        _.Cookie.HttpOnly = true;
                        _.Cookie.SameSite = SameSiteMode.Strict;
                        _.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        _.EventsType = typeof(CookieAuthenticationEventsHandler);
                    })
                .AddOpenIdConnect(
                    options =>
                    {
                        Configuration.Bind("Authentication:OpenIdConnect", options);

                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        
                        options.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                        options.Scope.Add(OpenIdConnectScope.UserImpersonation);
                        options.Scope.Add("roles");
                    
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.UseTokenLifetime = true;
                        options.SaveTokens = true;

                        options.TokenValidationParameters.RoleClaimType = "roles";
                        options.TokenValidationParameters.NameClaimType = "name";
                        options.TokenValidationParameters.ValidateLifetime = true;
                    }
                );

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("Test", policy => policy.RequireClaim(ClaimTypes.Role, "test_role"));
                });
        }

        private async Task OnValidatePrincipal(CookieValidatePrincipalContext context)
        {
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            
            

            app.UseStaticFiles();

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute("mvc", template: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}