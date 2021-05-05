using System.Linq;
using CommanderGQL.Data;
using CommanderGQL.Models;
using HotChocolate;
using HotChocolate.Types;

namespace CommanderGQL.GraphQL.Commands
{
    public class CommandType : ObjectType<Command>
    {
        protected override void Configure(IObjectTypeDescriptor<Command> descriptor)
        {
            descriptor.Description("Represents any executable command.");
            //descriptor.Field(c=>c.HowTo).Description("Brief howTo on how to do a certain task.");
            //descriptor.Field(c=>c.CommandLine).Description("The command line.");

            //Resolve c => c.Platform field (that is nested in the command object) with a resolver that is below
            descriptor.Field(c => c.Platform)
                      .ResolveWith<Resolvers>(c=>c.GetPlatform(default!, default!))
                      .UseDbContext<AppDbContext>()
                      .Description("The associated platform of this command.");
        }

        //Resolve the Platform child object that this CommandType needs
        private class Resolvers
        {
            public Platform GetPlatform(Command command, [ScopedService] AppDbContext context)
            {
                return context.Platforms.SingleOrDefault(p => p.Id == command.PlatformId);
            }
        }
    }
}