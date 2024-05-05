using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MarkdownDocumentGenerator
{
    public record Config
    {
        public string TargetBaseTypeName { get; init; } = ""; // ex. DTO.DTOBase

        public string ProjectPath { get; init; } = "";

        public string OutputMarkdownDirectory { get; init; } = "";

        public static void Validate([NotNull] Config? config)
        {
            ArgumentNullException.ThrowIfNull(config);

            var stringBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(config.TargetBaseTypeName))
            {
                stringBuilder.AppendLine($"{nameof(TargetBaseTypeName)} is required.");
            }

            if (string.IsNullOrWhiteSpace(config.ProjectPath))
            {
                stringBuilder.AppendLine($"{nameof(ProjectPath)} is required.");
            }

            if (string.IsNullOrWhiteSpace(config.OutputMarkdownDirectory))
            {
                stringBuilder.AppendLine($"{nameof(OutputMarkdownDirectory)} is required.");
            }

            if (stringBuilder.Length > 0)
            {
                throw new ArgumentException(stringBuilder.ToString());
            }
        }
    }
}
