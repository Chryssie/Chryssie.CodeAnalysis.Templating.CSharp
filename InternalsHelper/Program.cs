// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text;

using Mono.Cecil;

var paths = new string[]
{
     @"%UserProfile%\source\repos\Chryssie.CodeAnalysis.Templating.CSharp\Chryssie.CodeAnalysis.Templating.CSharp\Microsoft.CodeAnalysis.dll",
     @"%UserProfile%\source\repos\Chryssie.CodeAnalysis.Templating.CSharp\Chryssie.CodeAnalysis.Templating.CSharp\Microsoft.CodeAnalysis.CSharp.dll",
};

var internalsVisibleToTargets = new string[]
{
    "Chryssie.CodeAnalysis.Templating.CSharp",
    "Chryssie.CodeAnalysis.Templating.CSharp.UnitTests",
};


foreach (var originalPath in paths)
{
    var outputPath = Environment.ExpandEnvironmentVariables(originalPath);
    var inputPath = GetInputPath(outputPath);

    Console.WriteLine($"Upgrading {inputPath} > {outputPath}");

    var parameters = new ReaderParameters();
    var asm = ModuleDefinition.ReadModule(inputPath, parameters);

    var internalsVisibleToAttribute = asm.ImportReference(typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string), }))!;

    foreach (var internalsVisibleToTarget in internalsVisibleToTargets)
    {
        asm.Assembly.CustomAttributes.Add(new CustomAttribute(internalsVisibleToAttribute)
        {
            ConstructorArguments =
            {
                new CustomAttributeArgument(asm.TypeSystem.String, internalsVisibleToTarget),
            }
        });
    }

    asm.Write(outputPath);

}

static string GetInputPath(string outputPath)
{
    const string Suffix = ".Original";
    return string.Create(outputPath.Length + Suffix.Length, outputPath, (dest, outputPath) =>
    {
        var i = outputPath.LastIndexOf('.');
        var source = outputPath.AsSpan();

        source[..i].CopyTo(dest[..i]);
        Suffix.CopyTo(dest.Slice(i, Suffix.Length));
        source[i..].CopyTo(dest[(i + Suffix.Length)..]);
    });
}
