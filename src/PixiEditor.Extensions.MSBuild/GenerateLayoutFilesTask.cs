using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace PixiEditor.Extensions.MSBuild
{
    public class GenerateLayoutFilesTask : Task
    {
        [Required]
        public string AssemblyPath { get; set; } = null!;

        [Output]
        public string OutputPath { get; set; }

        public override bool Execute()
        {


            return !Log.HasLoggedErrors;
        }
    }
}
