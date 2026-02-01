namespace EFfluentify.Application.Ports
{
    public sealed class PipelineOptions
    {
        public string OutputDirectory { get; set; } = ".";
        public bool ManyFiles { get; set; } = true;
        public bool RemoveAnnotationsFromOriginal { get; set; } = false;
        public string RootNamespace { get; set; } = "EFfluentify.Configurations";
    }

}
