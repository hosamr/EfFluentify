using EFFluentify.Application.Models;

namespace EFFluentify.Cli
{
    public sealed record CommandRequest(
        IReadOnlyList<string> Inputs,
        PipelineOptions Options,
        bool PrintToConsole);
}
