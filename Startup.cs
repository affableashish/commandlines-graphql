using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommanderGQL.Data;
using CommanderGQL.GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GraphQL.Server.Ui.Voyager;
using CommanderGQL.GraphQL.Platforms;
using CommanderGQL.GraphQL.Commands;

namespace CommanderGQL
{
    public class Startup
    {
        private readonly IConfiguration Configuration;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        //We Add services (ASS) and Use and Map endpoints (UME) = ASSUME is .NET core!
        public void ConfigureServices(IServiceCollection services)
        {
            //AddPooledDbContextFactory because by default DbContext is not multithreaded and will cause execution errors if called at the same time. This solves that.
            services.AddPooledDbContextFactory<AppDbContext>(opts=>{
                opts.UseSqlServer(Configuration.GetConnectionString("CommanderConStr"));
            });

            services.AddGraphQLServer()
                    .AddQueryType<Query>() //Query is the class we added inside GraphQL
                    .AddMutationType<Mutation>()
                    .AddSubscriptionType<Subscription>()
                    .AddType<PlatformType>()
                    .AddType<CommandType>()
                    .AddFiltering()
                    .AddSorting()
                    .AddInMemorySubscriptions(); //Manage and track the subscribers in memory
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //We Add services (ASS) and Use and Map endpoints (UME) = ASSUME is .NET core!
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoint=>{
                endpoint.MapGraphQL(); //Setting up GraphQL pipeline. Goto this address: http://localhost:5000/graphql/ to hit this endpoint.
                endpoint.MapGraphQLVoyager(); //Setting up GraphQL voyager pipeline. Goto this address: http://localhost:5000/ui/voyager to hit this endpoint
            });

            //Add websockets
            app.UseWebSockets();

            // app.UseGraphQLVoyager(new VoyagerOptions()
            // {
            //     GraphQLEndPoint = "/graphql",
            // }, "/graphql-voyager");
        }
    }
}
