using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Basket.API.Mappings;

namespace Basket.API
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
            #region General Configuration
            services.AddScoped<IBasketRepository, BasketRepository>();
            #endregion

            #region gRPC configuration
            services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>
                (o => o.Address = new Uri(Configuration["GrpcSettings:DiscountUrl"]));
            services.AddScoped<DiscountGrpcService>();
            #endregion

            #region Redis Configuration
            services.AddStackExchangeRedisCache(options =>
            {
                string password = Configuration.GetValue<string>("CacheSettings:Password");
                string connectionString = Configuration.GetValue<string>("CacheSettings:ConnectionString");
                options.Configuration = $"{connectionString},password={password}";
            });
            #endregion

            #region AutoMapper Configuration
            services.AddAutoMapper(options =>
            {
                options.AddProfile<BasketProfile>();
            });
            #endregion

            #region MassTransit RabbitMQ Configuration
            services.AddMassTransit((configure) =>
            {
                configure.UsingRabbitMq((context, cfg) =>
                {
                    string rabbitMqHost = Configuration.GetValue<string>("MessageBusSettings:RabbitMqHost");

                    cfg.Host(rabbitMqHost);
                });
            });
            #endregion

            services.AddControllers();
            #region OpenAPI Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
