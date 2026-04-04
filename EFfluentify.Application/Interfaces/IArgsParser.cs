namespace EFfluentify.Application.Interfaces
{
    public interface IArgsParser
    {
        (List<string> inputs, string? output, bool manyFiles, bool remove, string rootNamespace) ParseOrThrow(string[] args);
    }
}
