# commandlines-graphql
GraphQL API in .NET core using HotChocolate. It's a very descriptive project (because it has lot of documentation in the comments) that outlines creating docker images, use EF code first approach to create and update Db schemas, use multi-threaded Db context, use GraphQL Query, Mutations and Subscriptions (using WebSockets). 
GraphQL is a query and manipulation language for APIs.
Is also runtime for fulfilling requests.

Step 1:
The architecture of the API looks like this:


Step 2:
It has 4 core concepts:
Schema	Types	Resolvers	Data Source
Describes the API in full.
	Query
	A resolver returns data	Database

Microservice

REST API
It is comprised of "types".
	Mutation
	for a given field.
It must have a "root query type".	Subscription
	
	Objects
	They can resolve to
	Enumeration
	"anything".
	Scalar

Objects example (It's represented in SDL-> Schema Definition language):

type: Car{
	id: ID!         // ! means it can't be null.
	make: String!
	model: String!
}

Scalar represents primitive data type. For eg, it can be:
Id (Special in GraphQL)
Int
String
Boolean
Float
etc.


Step 3: Add necessary nuget packages 

Ashishs-MacBook-Pro:Projects ashishkhanal$ dotnet new web -n CommanderGQL
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet add package HotChocolate.AspNetCore
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet add package HotChocolate.Data.EntityFramework
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet add package Microsoft.EntityFrameworkCore.Design
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet add package Microsoft.EntityFrameworkCore.SqlServer
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet add package GraphQL.Server.Ui.Voyager

Step 4:
Create docker-compose.yaml
This file is used to instruct setting up services (running containers).

version: '3'
services:
    sqlserver:
        image: "mcr.microsoft.com/mssql/server:2017-latest"
        environment: #ENV variables
            ACCEPT_EULA: "Y"
            SA_PASSORD: "pa55word!"
            MSSQL_PID: "Express" #Flavor of SQL server
        ports:
           - "1434:1433" #"ExternalPort:DockerPort"

Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ docker-compose up -d
-d flag: return commandline back to me

Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ docker-compose stop

This will spin up a SQL server for me! AMAZING!

Step 5:
Dependency Injection:


Service Container: Map associations between interfaces and implementation classes.

Default service container in .NET 5 is called IServiceProvider.

Startup.ConfigureServices -> Used to configure -> Service Container

Services Injected at Startup (ALSO the REASON why IConfiguration services DOESN'T need to be registered in the ConfigureServices method)

These services can be injected into Startup constructor when using IHostBuilder:
	IWebHostEnvironment
	IHostEnvironment
	IConfiguration
	

DI framework is responsible for creating an instance of the dependency and disposing of it.


Step 6:
Create AppDbContext derived from DbContext

Step 7:
Get Entity framework tools for the dotnet cli

dotnet tool install --global dotnet-ef
Or
dotnet tool update --global dotnet-ef


Step 8: Create migration
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet ef migrations add AddPlatformToDb

Step 9: Run migration
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet ef database update

Step 10:
A graph ql always needs to have a base query.
Create it:
    public class Query 
    {
        //Hotchocolate allows us to inject AppDbContext as a method parameter as opposed to constructor dependency injection that's statndard in .NET core
        public IQueryable<Platform> GetPlatform([Service] AppDbContext context)
        {
            return context.Platforms;
        }
    }

Step 11:
Add the GraphQL endpoint:
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL(); //Setting up GraphQL pipeline. Goto to hit this endpoint: http://localhost:5000/graphql/
            });
        }


Step 12:
Fire up this address:
http://localhost:5000/graphql/

Step: Run a query


Step 13:
To navigate through the Schema we need: GraphQL.Server.Ui.Voyager


Step 14:
Running as aliases in parallel causes errors:
Because DbContext is not multithreaded out of the box.
query{	{
  a: platform{	  "errors": [
    id	    {
    name	      "message": "Unexpected Execution Error",
  },	      "locations": [
  b: platform{	        {
    id	          "line": 6,
    name	          "column": 3
  }	        }
}	      ],
	      "path": [
	        "b"
	      ]
	    }
	  ],
	  "data": {
	    "a": [
	      {
	        "id": 1,
	        "name": ".NET 5"
	      },
	      {
	        "id": 2,
	        "name": "Docker"
	      },
	      {
	        "id": 3,
	        "name": "Windows"
	      }
	    ],
	    "b": null
	  }
	}

Solution:
Replace AddDbContext with AddPooledDbContextFactory in ConfigureServices

In the Query.cs
using HotChocolate.Data;

        [UseDbContext(typeof(AppDbContext))] //This method needs to get Db context from the pool, execute and give it back to the pool
        public IQueryable<Platform> GetPlatform([ScopedService] AppDbContext context)
        {
            return context.Platforms;
        }


Vs the old one:
        public IQueryable<Platform> GetPlatform([Service] AppDbContext context) //Service lifetime of Scoped
        {
            return context.Platforms;
        }


Step 15:
One to Many relationships

    //It has Many(Command) to One(Platform) relationship with Platform table. 
    //The many guy (Command) has 2 properties - Platform, PlatformId (easy way to remember) while the one guy (Platform) has just one property - Commands.
    public class Command
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string HowTo { get; set; }
        [Required]
        [MaxLength(64)]
        public string CommandLine { get; set; }
        [Required]
        public int PlatformId { get; set; } //Foreign key FOR Platform table
        public Platform Platform { get; set; } //I believe this is Navigation property and only used to retrieve Platform using the .Include extension method.
    }

    //It has One(Platform) to Many(Command) relationship with Commands table. 
    //The many guy (Command) has 2 properties - Platform, PlatformId (easy way to remember) while the one guy (Platform) has just one property - Commands.
    public class Platform{
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(24)]
        public string Name { get; set; }
        [MaxLength(32)]
        public string LicenseKey { get; set; }
        public ICollection<Command> Commands { get; set; } = new List<Command>(); //Navigation property
    }

Give instruction to EF on how to create schema:

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Perspective of Platform
            modelBuilder.Entity<Platform>() //EHWH - Order Matters
                        .HasMany(p=>p.Commands) //One to Many :public ICollection<Command> Commands { get; set; }
                        .WithOne(p=>p.Platform!) //Relationship with itself
                        .HasForeignKey(p=>p.PlatformId); //Weird it's in both places

            //Perspective of Command
            modelBuilder.Entity<Command>() //EHWH - Order Matters
                        .HasOne(c=>c.Platform) //One to Many
                        .WithMany(c=>c.Commands) //Relationship with itself
                        .HasForeignKey(c=>c.PlatformId); //Weird it's in both places
        }

Step 16:
Db Migrations: Remember using DEMAC and DEDU
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet ef migrations add AddCommandToDb //DEMAC 
Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet ef database update //DEDU


To get all Platforms WITH the commands, we need to tell the app to load child objects that are connected by Navigation property:

[UseProjection] //To load the child objects
public IQueryable<Platform> GetPlatform([ScopedService] AppDbContext context) //Service lifetime of Scoped
{
	return context.Platforms;
}

Test it:


And also in ConfigureServices:

services.AddGraphQLServer()
	.AddQueryType<Query>() //Query is the class we added inside GraphQL
	.AddProjections();

Step 17:
Navigate to Voyager to look at the schema!
http://localhost:5000/graphql-voyager
No / at the end though!

Add the command query in the Query.cs similar to Platform query.
Test the commands:


Step 18: 
Annotation vs Code first approaches
We have been doing annotation approach on exposing the objects so far.
(Annotate the model properties with attributes to provide GraphQL features.)

But now, let's refactor it to Code first approach
By introducing dedicated GraphQL schema types. As it allows us to separate concerns.

Step 19:
Create some types: PlatformType 
namespace CommanderGQL.GraphQL.Platforms
{
    //Inheriting from ObjectType(that comes from HotChocolate) and build on top of Platform to create GraphQL functionality to PlatformType
    public class PlatformType : ObjectType<Platform>
    {
        protected override void Configure(IObjectTypeDescriptor<Platform> descriptor)
        {
            descriptor.Description("Represents any software or service that has a CLI.");
            descriptor.Field(p=>p.LicenseKey).Ignore();
            descriptor.Field(p => p.Name).Description("Name of the software or service.");
        }
    }
}

Step 20:
Write resolvers now. 
Resolver basically tells how to get the required objects.

Now we use Resolver and can get rid of Projections.

    public class PlatformType : ObjectType<Platform>
    {
        protected override void Configure(IObjectTypeDescriptor<Platform> descriptor)
        {
            descriptor.Description("Represents any software or service that has a CLI.");
            descriptor.Field(p=>p.LicenseKey).Ignore();
            //descriptor.Field(p => p.Name).Description("Name of the software or service.");

		 //Resolve p=>p.Commands field (that is nested in the Platform object) with a resolver that is below
            descriptor.Field(p=>p.Commands)
                      .ResolveWith<Resolvers>(p => p.GetCommands(default!, default!)) //! can't be null
                      .UseDbContext<AppDbContext>()
                      .Description("This is the list of available commands for this platform");
        }

        private class Resolvers{
            public IQueryable<Command> GetCommands(Platform platform, [ScopedService] AppDbContext context)
            {
                return context.Commands.Where(c=>c.PlatformId == platform.Id);
            }
        }
    }

Notice No [Use Projection] here in the Query.cs:
        //Hotchocolate allows us to inject AppDbContext as a method parameter as opposed to constructor dependency injection that's statndard in .NET core
        [UseDbContext(typeof(AppDbContext))] //This method needs to get Db context from the pool, execute and give it back to the pool
        public IQueryable<Platform> GetPlatform([ScopedService] AppDbContext context) //Service lifetime of Scoped
        {
            return context.Platforms;
        }

Notice No AddProjections() here in the Startup.cs Configure services method either:
            services.AddGraphQLServer()
                    .AddQueryType<Query>() //Query is the class we added inside GraphQL
                    .AddType<CommandType>()
                    .AddType<PlatformType>();

Step 21:
This issue (platform not being used in the command query or voyager) fucked my mind for few days. I couldn't spot it until I loaded the project into Visual Studio and noticed what was causing the issue.


The issue was that in the Startup.cs




Pay attention to where CommandType is being referenced from:


Putting this at the top fixed this issue (I was using some random namespace for CommandType from System which had messed this up):
using CommanderGQL.GraphQL.Commands;

That fixed the issue!

Step 22:
Using sorting and filtering:

//This is resolver
[UseDbContext(typeof(AppDbContext))]
[UseSorting]
[UseFiltering]
public IQueryable<Command> GetCommand([ScopedService] AppDbContext context)
{
	return context.Commands;
}

services.AddGraphQLServer()
	   .AddQueryType<Query>() //Query is the class we added inside GraphQL
	   .AddType<PlatformType>()
	   .AddType<CommandType>()
	   .AddFiltering()
	   .AddSorting();

Ashishs-MacBook-Pro:CommanderGQL ashishkhanal$ dotnet watch run


Filtering example query:	Sorting example query
query{	query{
  command(where: {platformId: {eq: 1}})	  platform(order: {name: ASC}){
  {	    name
    id	  }
    howTo	}
    commandLine
    platform{
      id
      name
    }
  }
}
{	{
  "data": {	  "data": {
    "command": [	    "platform": [
      {	      {
        "id": 1,	        "name": ".NET 5"
        "howTo": "Build a project",	      },
        "commandLine": "dotnet build",	      {
        "platform": {	        "name": "Docker"
          "id": 1,	      },
          "name": ".NET 5"	      {
        }	        "name": "Windows"
      },	      }
      {	    ]
        "id": 2,	  }
        "howTo": "Run a project",	}
        "commandLine": "dotnet run",
        "platform": {
          "id": 1,
          "name": ".NET 5"
        }
      }
    ]
  }
}

Step 23:
Mutations:
Changing data: creating data, updating data, and deleting data.

Side Note: We're using records that are reference type used instead of classes or structs. They use value based equality.
For eg: 2 variables of a record type are equal if the record type definitions are identical and if for every field, the values in both records are equal.

Mutation.cs	Startup.cs
        //Name of the mutation	            services.AddGraphQLServer()
        //Input -> AddPlatformInput record type that takes field: string Name	                    .AddQueryType<Query>() //Query is the class we added inside GraphQL
        //Payload/ Output -> AddPlatformPayload record type that takes field: Platform platform	                    .AddMutationType<Mutation>()
        [UseDbContext(typeof(AppDbContext))] //For multithreaded Db context.	                    .AddType<PlatformType>()
        public async Task<AddPlatformPayload> AddPlatformAsync(AddPlatformInput input, [ScopedService] AppDbContext context)	                    .AddType<CommandType>()
        {	                    .AddFiltering()
            var platform = new Platform	                    .AddSorting();
            {
                Name = input.Name
            };

            context.Platforms.Add(platform);
            await context.SaveChangesAsync();

            return new AddPlatformPayload(platform);
        }

In voyager:
addPlatform name coming from AddPlatformAsync mutation that takes AddPlatformInput and returns AddPlatformPayload.

public record AddPlatformInput(string Name);
public record AddPlatformPayload(Platform platform);





Example usage:
Request	Response
mutation{	{
  addPlatform(input:{	  "data": {
    name: "Ubuntu"	    "addPlatform": {
  })	      "platform": {
  {	        "id": 4,
    platform{	        "name": "Ubuntu"
      id	      }
      name	    }
    }	  }
  }	}
}

AddCommandAsync

Request	Response
mutation{	{
  addCommand(input:{	  "data": {
    howTo: "Perform directory listing"	    "addCommand": {
    commandLine: "ls"	      "command": {
    platformId: 4	        "id": 5,
  })	        "howTo": "Perform directory listing",
  {	        "commandLine": "ls",
    command{	        "platform": {
      id	          "name": "Ubuntu"
      howTo	        }
      commandLine	      }
      platform{	    }
        name	  }
      }	}
    }
  }
}


Step 24:
Subscriptions
They rely on WebSockets.

Let's say in an ecommerce app, someone creates an order.
We want let's say Warehouse management service to subscribe to subscription endpoint and be notified.

WebSocket connection is duplex and open/ persistent for the duration of the session.
Used for real time, chat type apps.

namespace CommanderGQL.GraphQL
{
    public class Subscription
    {
        [Subscribe] //Means we can subscribe to this method
        [Topic] //Type of subscription we can have
        public Platform OnPlatformAdded([EventMessage] Platform platform)
        {
            return platform;
        }
    }
}


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Add websockets
            app.UseWebSockets();
	       }

Configure Services
            services.AddGraphQLServer()
                    .AddQueryType<Query>() //Query is the class we added inside GraphQL
                    .AddMutationType<Mutation>()
                 .AddSubscriptionType<Subscription>()
                    .AddType<PlatformType>()
                    .AddType<CommandType>()
                    .AddFiltering()
                    .AddSorting()
                    .AddInMemorySubscriptions(); //Manage and track the subscribers in memory


In Mutation.cs
        //Name of the mutation
        //Input -> AddPlatformInput
        //Payload/ Output -> AddPlatformPayload
        [UseDbContext(typeof(AppDbContext))] //For multithreaded Db context.
        public async Task<AddPlatformPayload> AddPlatformAsync(AddPlatformInput input, 
                                            [ScopedService] AppDbContext context,
                                            [Service] ITopicEventSender eventSender, //Use to send the event. Comes from HotChocolate.Subscriptions
                                            CancellationToken cancellationToken) //Allows us to cancel async method calls if they're hanging for eg. It comes from System.Threading
        {
            var platform = new Platform
            {
                Name = input.Name
            };

            context.Platforms.Add(platform);
            await context.SaveChangesAsync(cancellationToken);

            await eventSender.SendAsync(nameof(Subscription.OnPlatformAdded), platform, cancellationToken);

            return new AddPlatformPayload(platform);
        }


To test it:
Run this and wait:
subscription{
   onPlatformAdded{
      id
      name
   }
}

Run this:
mutation{	{
  addPlatform(input:{	  "data": {
    name: "SUSE"	    "addPlatform": {
  })	      "platform": {
  {	        "id": 6,
    platform{	        "name": "SUSE"
      id	      }
      name	    }
    }	  }
  }	}
}

The subscription should get data right after a new platform is added.
subscription{	{
   onPlatformAdded{	  "data": {
      id	    "onPlatformAdded": {
      name	        "id": 6,
   }	        "name": "SUSE"
}	      }
	  }
	}

![image](https://user-images.githubusercontent.com/30603497/118218994-91195900-b446-11eb-87d2-f334f03d5b6f.png)
