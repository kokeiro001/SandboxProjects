﻿using System.Xml.Linq;
using Dumpify;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;

namespace MarkdownDocumentGenerator
{
    internal class Program
    {
        static readonly string TargetBaseClassName = "DTO.DTOBase";

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var dtoProjectFilePath = configuration["DTOProjectPath"];

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(dtoProjectFilePath);

            // プロジェクト内のすべてのソースコードファイルを取得
            var documents = project.Documents ?? [];

            var classInfos = new List<ClassInfo>();

            // プロジェクト内の各ソースコードファイルに対して解析を実行
            foreach (var document in documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var semanticModel = await document.GetSemanticModelAsync();

                if (syntaxTree is null || semanticModel is null)
                {
                    continue;
                }

                // 解析して特定のクラスを継承しているクラスの一覧を取得
                var root = syntaxTree.GetCompilationUnitRoot();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classSyntax in classes)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

                    if (classSymbol is null)
                    {
                        continue;
                    }

                    // TODO: 名前空間込みの名前にする
                    if (!IsInheritClass(classSymbol, TargetBaseClassName))
                    {
                        continue;
                    }

                    Console.WriteLine($"Class {classSymbol.Name} inherits from {TargetBaseClassName}");

                    var classInfo = new ClassInfo(classSymbol, semanticModel);
                    //Console.WriteLine($"Documentation for class {classInfo.Name}: {classInfo.Summary}");

                    classInfo.CollectProperties();
                    classInfos.Add(classInfo);
                }
            }

            classInfos.DumpConsole();
        }

        private static bool IsInheritClass(INamedTypeSymbol classSymbol, string classFullName)
        {
            while (classSymbol.BaseType != null)
            {
                if (classSymbol.BaseType.ToString() == classFullName)
                {
                    return true;
                }

                classSymbol = classSymbol.BaseType;
            }

            return false;
        }
    }

    class DocumentationComment
    {
        private readonly XElement? xmlElement;

        public DocumentationComment(string xml)
        {

            try
            {
                xmlElement = XElement.Parse(xml);
            }
            catch
            {
            }
        }

        public string GetSummary()
        {
            return GetTag("summary");
        }

        private string GetTag(string tag)
        {
            return xmlElement?.Element(tag)?.Value?.Trim() ?? "";
        }
    }

    class ClassInfo
    {
        private readonly DocumentationComment documentationComment;
        private readonly INamedTypeSymbol classSymbol;
        private readonly SemanticModel semanticModel;

        public ClassInfo(INamedTypeSymbol classSymbol, SemanticModel semanticModel)
        {
            this.classSymbol = classSymbol;
            this.semanticModel = semanticModel;
            var docComment = classSymbol.GetDocumentationCommentXml() ?? "";
            documentationComment = new DocumentationComment(docComment);
        }

        public string Name => classSymbol.Name;

        public string Namespace => classSymbol.ContainingNamespace?.ToString() ?? "";

        public string Summary => documentationComment.GetSummary();

        public List<PropertyInfo> Properties { get; set; } = [];

        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

        public List<ClassInfo> AssociationClasses { get; set; } = [];

        public void CollectProperties()
        {
            INamedTypeSymbol? currentClassSymbol = classSymbol;

            while (currentClassSymbol != null)
            {
                foreach (var propertySymbol in currentClassSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var propertyInfo = new PropertyInfo(propertySymbol, semanticModel);

                    //Console.WriteLine($"Documentation for member {propertyInfo.Name}: Summary: {propertyInfo.Summary}");

                    Properties.Add(propertyInfo);
                }

                currentClassSymbol = currentClassSymbol.BaseType;
            }

        }
    }

    class PropertyInfo
    {
        private readonly IPropertySymbol propertySymbol;
        private readonly SemanticModel semanticModel;
        private readonly DocumentationComment propertyDocComment;

        public PropertyInfo(IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            this.propertySymbol = propertySymbol;
            this.semanticModel = semanticModel;

            var propertyDocumentationCommentXml = propertySymbol.GetDocumentationCommentXml() ?? "";

            propertyDocComment = new DocumentationComment(propertyDocumentationCommentXml);
        }

        public string Name => propertySymbol.Name;

        public string TypeName => GetTypeName();

        public string Summary => propertyDocComment.GetSummary();

        private string GetTypeName()
        {
            var listTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            if (IsListType(listTypeSymbol!))
            {
                var firstTypeArgument = ((INamedTypeSymbol)propertySymbol.Type).TypeArguments.First();
                var typeName = firstTypeArgument.Name;

                return $"List<{typeName}>";
            }

            if (propertySymbol.Type.Kind == SymbolKind.ArrayType
                && propertySymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                return $"{arrayTypeSymbol.ElementType.Name}[]";
            }

            return propertySymbol.Type.Name;
        }

        private bool IsListType(INamedTypeSymbol listTypeSymbol)
        {
            // 型シンボルが List<T> かどうかを判断
            return propertySymbol.Type.OriginalDefinition.Equals(listTypeSymbol, SymbolEqualityComparer.Default)
                   && propertySymbol.Type is INamedTypeSymbol namedType
                   && namedType.IsGenericType
                   && namedType.TypeArguments.Length == 1;
        }
    }
}
