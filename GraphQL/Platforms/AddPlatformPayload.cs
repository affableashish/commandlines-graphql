using CommanderGQL.Models;

namespace CommanderGQL.GraphQL.Platforms
{
    //Expecting to return AddPlatformPayload object that takes platform object
    public record AddPlatformPayload(Platform platform);
}