using HiveMind;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SC2APIProtocol;
using System;

namespace Api
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
            services.AddControllers();
            services.AddSingleton<IWebSocketWrapper, WebSocketWrapper>();
            services.AddSingleton<IConnectionService, ConnectionService>();
            services.AddSingleton<GameStarter, GameStarter>();
            services.AddSingleton<IConstantManager>(_=>new ConstantManager(Race.Terran));
            services.AddSingleton<IGameDataService, GameDataService>();
            services.AddSingleton<IBuildingManager, BuildingManager>();
            services.AddSingleton<IUnitBuilder, UnitBuilder>();
            services.AddSingleton<IArmyManager, ArmyManager>();
            services.AddSingleton<IBuildQueue, BuildQueue>();
            services.AddSingleton<Game>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
