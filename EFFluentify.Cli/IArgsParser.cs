namespace EFFluentify.Cli
{
    public interface IArgsParser
    {
        CommandRequest ParseOrThrow(string[] args);
    }
}
