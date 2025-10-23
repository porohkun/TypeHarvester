#! "net8.0"
// ↑ можно указать свою целевую платформу (для dotnet-script)

#load "Abstractions/ITemplateLine.csx"
#load "TT/CodeBlockLine.csx"
#load "TT/Directive.csx"
#load "TT/InlineExpression.csx"
#load "TT/SimpleLine.csx"
#load "TT/TemplateLineDetector.csx"
#load "TT/TtGenerator.csx"

foreach (var templatePath in Directory.GetFiles("Templates", "*.tt"))
{
    var template = File.ReadAllText(templatePath);
    var className = Path.GetFileNameWithoutExtension(templatePath) + "Generator";
    var generated = TtGenerator.GenerateCode(template, className);
    File.WriteAllText(Path.Combine("Generators", className + ".g.cs"), generated);
}
