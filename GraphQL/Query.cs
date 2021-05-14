using System.Linq;
using CommanderGQL.Data;
using CommanderGQL.Models;
using HotChocolate;
using HotChocolate.Data;

namespace CommanderGQL.GraphQL{
    public class Query 
    {
        //Hotchocolate allows us to inject AppDbContext as a method parameter as opposed to constructor dependency injection that's statndard in .NET core
        [UseDbContext(typeof(AppDbContext))] //This method needs to get Db context from the pool, execute and give it back to the pool
        [UseSorting]
        [UseFiltering]
        public IQueryable<Platform> GetPlatform([ScopedService] AppDbContext context) //Service lifetime of Scoped
        {
            return context.Platforms;
        }

        //This is resolver
        [UseDbContext(typeof(AppDbContext))]
        [UseSorting]
        [UseFiltering]
        public IQueryable<Command> GetCommand([ScopedService] AppDbContext context)
        {
            return context.Commands;
        }
    }
}