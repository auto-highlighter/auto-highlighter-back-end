using auto_highlighter_back_end.DataAccess;
using auto_highlighter_back_end.Filters;
using auto_highlighter_back_end.Repository;
using auto_highlighter_back_end.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace auto_highlighter_back_end
{
    public class Startup
    {

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITempHighlightRepo, TempHighlightRepo>();
            services.AddSingleton<IVideoProcessService, VideoProcessService>();
            services.AddSingleton<IMessageQueueService, MessageQueueService>();

            services.AddControllers(options =>
            {
                options.Filters.Add<RateLimitActionFilter>();
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "auto_highlighter_back_end", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "auto_highlighter_back_end v1"));
            }


            string ffmpegLocation = Path.Combine(_env.ContentRootPath, _config["FFmpegSettings:ExecutableLocation"]);

            if (!Directory.Exists(ffmpegLocation))
            {
                Directory.CreateDirectory(ffmpegLocation);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegLocation);
            }

            FFmpeg.SetExecutablesPath(ffmpegLocation, ffmpegExeutableName: "FFmpeg");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            IMessageQueueService mq = app.ApplicationServices.GetService<IMessageQueueService>();
            mq.ReceiveMessagesAsync();
        }
    }
}
