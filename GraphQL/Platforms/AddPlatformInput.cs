namespace CommanderGQL.GraphQL.Platforms
{
    //To create a platform, we need provide Name, LiceseKey or Commands. To keep it simple, we'll just create it with a name.
    public record AddPlatformInput(string Name);
}