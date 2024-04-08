namespace MarkdownDocumentGenerator
{
    public record Config
    {
        public string TargetBaseClassName { get; set; } = "DTO.DTOBase";

        public string TargetBaseNamespace { get; set; } = "DTO";

        public string ProjectPath { get; set; } = "";

        public string OutputMarkdownDirectory { get; set; } = "";
    }
}
