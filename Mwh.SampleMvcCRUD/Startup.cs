using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Mwh.Sample.Common.Clients;
using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Repositories;
using Mwh.Sample.Core.Data.Models;
using Mwh.Sample.Core.Data.Repository;
using Mwh.Sample.Core.WebApi.Extensions;

namespace Mwh.Sample.Core.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) 
        {
            Configuration = configuration;
            ConfirmDatabaseCreation();
        }


        private void ConfirmDatabaseCreation()
        {
            var dbOptions = new DbContextOptionsBuilder<Data.Models.EmployeeContext>()
                .UseInMemoryDatabase("employee")
                .Options;
            var context = new EmployeeContext(dbOptions);
            context.Database.EnsureDeletedAsync();
            context.Database.EnsureCreatedAsync();
            context.Employees.Add(new Employee() { Name = "John Doe", Age = 25, Country = "USA", DepartmentId = 1, State = "TX" });
            context.Employees.Add(new Employee() { Name = "Sam Spade", Age = 45, Country = "USA", DepartmentId = 1, State = "TX" });
            context.Employees.Add(new Employee() { Name = "Rick Blaine", Age = 55, Country = "USA", DepartmentId = 1, State = "TX" });
            context.Employees.Add(new Employee() { Name = "Victor Laszlo", Age = 55, Country = "USA", DepartmentId = 2, State = "TX" });
            context.Employees.Add(new Employee() { Name = "Louis Renault", Age = 50, Country = "USA", DepartmentId = 3, State = "TX" });
            context.SaveChanges();
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCustomSwagger();

            app.UseHttpContext();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Data.Models.EmployeeContext>(options => options
                .EnableSensitiveDataLogging(Configuration.GetValue<bool>("Logging:EnableSqlParameterLogging"))
                .UseInMemoryDatabase("employee"));

            services.AddScoped<IEmployeeDB, EmployeeDB>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IEmployeeClient, EmployeeClient>();
            services.AddCustomSwagger();
            services.AddMvc();
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }
    }
}
