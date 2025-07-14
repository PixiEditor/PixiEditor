using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Mono.Cecil;

namespace PixiEditor.Api.CGlueMSBuild
{
    public class GenerateCGlueTask : Microsoft.Build.Utilities.Task
    {
        /// <summary>
        /// Path of extension assembly.
        /// </summary>
        [Required]
        public string AssemblyPath { get; set; } = default!;

        /// <summary>
        /// Path where output C files should be placed.
        /// </summary>
        [Required]
        public string OutputPath { get; set; } = default!;

        /// <summary>
        /// Path of main interop C file
        /// </summary>
        [Required]
        public string InteropCFilePath { get; set; } = default!;

        public string EncryptionKey { get; set; } = string.Empty;

        public string EncryptionIv { get; set; } = string.Empty;

        /// <summary>
        ///     /// Encryption key for resources.
        /// </summary>

        public override bool Execute()
        {
            try
            {
                var assemblyFileName = Path.GetFileName(AssemblyPath);
                var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);

                if (!Directory.Exists(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                }
                else
                {
                    foreach (var file in Directory.GetFiles(OutputPath, "*.c"))
                    {
                        if (file == InteropCFilePath)
                        {
                            continue;
                        }

                        File.Delete(file);
                    }
                }

                var generator = new CApiGenerator(File.ReadAllText(InteropCFilePath), EncryptionKey, EncryptionIv, (message) => Log.LogMessage(MessageImportance.High, message));

                string directory = Path.GetDirectoryName(AssemblyPath)!;

                string generated = generator.Generate(assembly, directory);
                Log.LogMessage(MessageImportance.High, $"Generated C API: {Path.GetFullPath(Path.Combine(OutputPath, "interop.c"))}");
                File.WriteAllText(Path.Combine(OutputPath, "interop.c"), generated);

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
