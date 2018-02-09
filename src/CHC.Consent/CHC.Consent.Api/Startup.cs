using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace CHC.Consent.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(gen =>
            {
                gen.SwaggerDoc("v1", new Info {Title = "Api"});
                gen.DescribeAllEnumsAsStrings();
            });

            var knownIdentifierTypes = new[]
                {IdentifierType.NhsNumber, IdentifierType.BradfordHospitalNumber, IdentifierType.DateOfBirth,};
            services.AddSingleton(typeof(IStore<>), typeof(InMemoryStore<>));
            services.AddSingleton(MakePersonStore());
            services.AddSingleton(knownIdentifierTypes.AsQueryable());
            services.AddScoped<IdentityRepository>();
            
        }

        private static IStore<Person> MakePersonStore()
        {
            var peopleStore = new InMemoryStore<Person>();
            peopleStore.OnItemAdded += (store, person) => person.Id = store.Contents.Count;
            return peopleStore;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
            });
        }
    }
}