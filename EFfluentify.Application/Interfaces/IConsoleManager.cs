namespace EFfluentify.Application.Interfaces
{
    public interface IConsoleManager
    {
        void WriteLine(string message);
        void WriteError(string message);
        string? ReadLine();
    }
}
