using System.Threading;
using System.Threading.Tasks;
using CommanderGQL.Data;
using CommanderGQL.GraphQL.Commands;
using CommanderGQL.GraphQL.Platforms;
using CommanderGQL.Models;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Subscriptions;

namespace CommanderGQL.GraphQL
{
    public class Mutation
    {
        //Name of the mutation
        //Input -> AddPlatformInput
        //Payload/ Output -> AddPlatformPayload
        [UseDbContext(typeof(AppDbContext))] //For multithreaded Db context.
        public async Task<AddPlatformPayload> AddPlatformAsync(AddPlatformInput input, 
                                            [ScopedService] AppDbContext context,
                                            [Service] ITopicEventSender eventSender, //Use to send the event. Comes from Hotchocolate.Subscriptions
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

        [UseDbContext(typeof(AppDbContext))] //For multithreaded Db context
        public async Task<AddCommandPayload> AddCommandAsync(AddCommandInput input, [ScopedService] AppDbContext context)
        {
            var command = new Command
            {
                HowTo = input.HowTo,
                CommandLine = input.CommandLine,
                PlatformId = input.PlatformId
            };

            context.Commands.Add(command);
            await context.SaveChangesAsync();

            return new AddCommandPayload(command);
        }
    }
}