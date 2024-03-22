// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.Emit;
//
// namespace Krialys.Test
// {
//     public static class RoslynTest
//     {
//         public static void Compiler()
//         {
//             var compilation = CSharpCompilation.Create("test");
//
//             compilation = compilation.WithOptions(
//                 new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Debug, platform: Platform.X64));
//
//             var tree = CSharpSyntaxTree.ParseText(
//                 @"using System;
//                 using System.IO;
//                 class Program
//                 {
//                     static void Main() => System.Console.WriteLine (""Hello"");
//                 }"
//             );
//             compilation = compilation.AddSyntaxTrees(tree);
//
//             string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
//             string[] trustedAssemblyPaths = trustedAssemblies.Split(Path.PathSeparator);
//
//             var references = trustedAssemblyPaths.Select(path => MetadataReference.CreateFromFile(path));
//             compilation = compilation.AddReferences(references);
//
//             var runtimeRef = MetadataReference.CreateFromFile(Path.Combine(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.16", "System.Runtime.dll"));
//             compilation = compilation.AddReferences(runtimeRef);
//
//             EmitResult result = compilation.Emit(@"c:\temp\test.dll");
//             Console.WriteLine(result.Success);
//         }
//     }
// }
