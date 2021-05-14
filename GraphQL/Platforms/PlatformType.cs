using System.Linq;
using CommanderGQL.Data;
using CommanderGQL.Models;
using HotChocolate;
using HotChocolate.Types;

namespace CommanderGQL.GraphQL.Platforms
{
    //Inheriting from ObjectType(that comes from HotChocolate) and build on top of Platform to create GraphQL functionality to PlatformType
    public class PlatformType : ObjectType<Platform>
    {
        protected override void Configure(IObjectTypeDescriptor<Platform> descriptor)
        {
            descriptor.Description("Represents any software or service that has a CLI.");
            descriptor.Field(p=>p.LicenseKey).Ignore(); // Don't expose it in the API
            descriptor.Field(p => p.Name).Description("Name of the software or service.");

            //Resolve p=>p.Commands field (that is nested in the Platform object) with a resolver that is below
            descriptor.Field(p=>p.Commands)
                      .ResolveWith<Resolvers>(p => p.GetCommands(default!, default!)) //! can't be null
                      .UseDbContext<AppDbContext>()
                      .Description("This is the list of available commands for this platform");
        }

        private class Resolvers
        {
            public IQueryable<Command> GetCommands(Platform platform, [ScopedService] AppDbContext context)
            {
                return context.Commands.Where(c=>c.PlatformId == platform.Id);
            }
        }
    }
}