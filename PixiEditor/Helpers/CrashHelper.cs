using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers
{
    public static class CrashHelper
    {
        public static void SaveCrashInfo(Exception e)
        {
            StringBuilder builder = new System.Text.StringBuilder();
            DateTime currentTime = DateTime.Now;

            builder
                .Append($"PixiEditor crashed on {currentTime:yyyy.MM.dd} at {currentTime:HH:mm:ss}\n\n")
                .Append("-------Crash message-------\n")
                .Append(e.GetType().ToString())
                .Append(": ")
                .Append(e.Message);
            {
                var innerException = e.InnerException;
                while (innerException != null)
                {
                    builder
                        .Append("\n-----Inner exception-----\n")
                        .Append(innerException.GetType().ToString())
                        .Append(": ")
                        .Append(innerException.Message);
                    innerException = innerException.InnerException;
                }
            }

            builder
                .Append("\n\n-------Stack trace-------\n")
                .Append(e.StackTrace);
            {
                var innerException = e.InnerException;
                while (innerException != null)
                {
                    builder
                        .Append("\n-----Inner exception-----\n")
                        .Append(innerException.StackTrace);
                    innerException = innerException.InnerException;
                }
            }

            string filename = $"crash-{currentTime:yyyy-MM-dd_HH-mm-ss_fff}.txt";
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PixiEditor",
                "crash_logs");
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, filename), builder.ToString());
        }
    }
}