﻿using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sikiro.Infrastructure.Msg.Service;
using Sikiro.MicroService.Extension;
using Sikiro.MicroService.Extension.Attributes;
using Sikiro.MicroService.Extension.SkyApm;
using Sikiro.Nosql.Mongo;
using Sikiro.Nosql.Redis;

namespace Sikiro.Infrastructure.Msg
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<RpcGolbalExceptionAttribute>();
            });

            services.AddHealthChecks();

            services.AddSingleton(new RedisRepository(Configuration["redisUrl"]));

            services.AddSingleton(Configuration);

            services.AddSingleton(new SmsService(Configuration["Sms:key"], Configuration["Sms:secret"], Configuration["Sms:sign"], Configuration["Sms:code"], Configuration["Sms:foreignsign"], Configuration["Sms:foreigncode"]));

            services.AddHttpContextAccessor();

            services.UseSkyApm();

            services.AddSingleton(new MongoRepository(Configuration["MongoDbUrl"]));

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "基础设施-消息服务";
                    document.Info.Description = Assembly.GetExecutingAssembly().GetName(true).Name;
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            app.UseHealthChecks("/health");

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseOpenApi();
            app.UseSwaggerUi3(options =>
            {
                options.Path = "";
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseConsul(lifetime, Configuration);
        }
    }
}
