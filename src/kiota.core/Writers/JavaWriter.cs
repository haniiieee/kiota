using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace kiota.core
{
    public class JavaWriter : LanguageWriter
    {
        private readonly IPathSegmenter segmenter;

        public JavaWriter(string rootPath, string clientNamespaceName)
        {
            segmenter = new JavaPathSegmenter(rootPath, clientNamespaceName);
        }
        public override IPathSegmenter PathSegmenter => segmenter;

        public override string GetParameterSignature(CodeParameter parameter)
        {
            return $"@javax.annotation.{(parameter.Optional ? "Nullable" : "Nonnull")} final {GetTypeString(parameter.Type)} {parameter.Name}";
        }

        public override string GetTypeString(CodeType code)
        {
            var typeName = TranslateType(code.Name, code.Schema);
            if (code.ActionOf)
            {
                return $"java.util.function.Consumer<{typeName}>";
            }
            else
            {
                return typeName;
            }
        }

        public override string TranslateType(string typeName, OpenApiSchema schema)
        {
            switch (typeName)
            {//TODO we're probably missing a bunch of type mappings
                case "integer": return "Integer";
                case "boolean": return "Boolean";
                case "string": return "String";
                case "object": return "Object";
                case "array": return $"{TranslateType(schema.Items.Type, schema.Items)}[]";
                default: return typeName ?? "Object";
            }
        }

        public override void WriteCodeClassDeclaration(CodeClass.Declaration code)
        {
            if(code?.Parent?.Parent is CodeNamespace ns) {
                WriteLine($"package {ns.Name};");
                WriteLine();
                code.Usings
                    .Select(x => $"import {x.Name}.{x.Declaration.Name.ToFirstCharacterUpperCase()};")
                    .Distinct()
                    .ToList()
                    .ForEach(x => WriteLine(x));
            }
            //TODO: missing javadoc
            WriteLine($"public class {code.Name.ToFirstCharacterUpperCase()} {{");
            IncreaseIndent();
        }

        public override void WriteCodeClassEnd(CodeClass.End code)
        {
            DecreaseIndent();
            WriteLine("}");
        }

        public override void WriteIndexer(CodeIndexer code)
        {
            var method = new CodeMethod(code) {
                Name = "get",
                ReturnType = code.ReturnType,
                IsAsync = false,
                MethodKind = CodeMethodKind.IndexerBackwardCompatibility,
            };
            method.AddParameter(new CodeParameter(method) {
                        Name = "position",
                        Type = code.IndexType,
                        Optional = false,
                    });
            WriteMethod(method);
        }

        public override void WriteMethod(CodeMethod code)
        {
            //TODO javadoc
            WriteLine(code.ReturnType.IsNullable ? "@javax.annotation.Nullable" : "@javax.annotation.Nonnull");
            WriteLine($"{GetAccessModifier(code.Access)} {(code.IsAsync ? "java.util.concurrent.CompletableFuture<" : string.Empty)}{GetTypeString(code.ReturnType).ToFirstCharacterUpperCase()}{(code.IsAsync ? ">" : string.Empty)} {code.Name.ToFirstCharacterLowerCase()}({string.Join(',', code.Parameters.Select(p=> GetParameterSignature(p)).ToList())}) {{");
            IncreaseIndent();
            switch(code.MethodKind) {
                case CodeMethodKind.IndexerBackwardCompatibility:
                    WriteLine($"final {code.ReturnType.Name} builder = new {code.ReturnType.Name}();");
                    WriteLine("builder.currentPath = this.currentPath + this.pathSegment + \"/\" + position;");
                    WriteLine("return builder;");
                break;
                default:
                    WriteLine("return null;");
                break;
            }
            DecreaseIndent();
            WriteLine("}");
        }

        public override void WriteProperty(CodeProperty code)
        {
            //TODO: missing javadoc
            var defaultValue = string.IsNullOrEmpty(code.DefaultValue) ? string.Empty : $" = {code.DefaultValue}";
            WriteLine(code.Type.IsNullable ? "@javax.annotation.Nullable" : "@javax.annotation.Nonnull");
            WriteLine($"{GetAccessModifier(code.Access)}{(code.ReadOnly ? " final " : " ")}{GetTypeString(code.Type)} {code.Name}{defaultValue};");
        }

        public override void WriteType(CodeType code)
        {
            Write(GetTypeString(code), includeIndent: false);
        }
        public override string GetAccessModifier(AccessModifier access)
        {
            return (access == AccessModifier.Public ? "public" : (access == AccessModifier.Protected ? "protected" : "private"));
        }
    }
}
