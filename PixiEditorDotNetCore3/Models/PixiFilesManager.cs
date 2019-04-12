using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PixiEditorDotNetCore3.Models
{
    public static class PixiFilesManager
    {

        public static string TempFolderPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"PixiEditor\Temp"); }
        }

        public static string RedoStackPath
        {
            get { return Path.Combine(TempFolderPath, @"RedoStack"); }
        }

        public static string UndoStackPath
        {
            get { return Path.Combine(TempFolderPath, @"UndoStack"); }
        }

        /// <summary>
        /// Saves object to file on disk using binary formatter
        /// </summary>
        /// <param name="obj">Object to be saved</param>
        public static void SaveObjectToJsonFile<T>(T obj, string fileName) where T : new()
        {
            try
            {
                SaveSerializedObjectToFile(obj, fileName);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        public static void RemoveFile(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Removes all files from directory
        /// </summary>
        /// <param name="path"></param>
        public static void ClearDirectory(string path)
        {
            string[] filesInDirectory = Directory.GetFiles(path);
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                File.Delete(filesInDirectory[i]);
            }
        }

        private static void SaveSerializedObjectToFile(object obj, string filename)
        {
            using (TextWriter writer = new StreamWriter(filename, false))
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(obj);
                writer.Write(contentsToWriteToFile);
            }
        }

        public static T ReadObjectFromFile<T>(string filePath) where T : new()
        {
            using (TextReader reader = new StreamReader(filePath))
            {
                var fileContent = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContent);
            }
        }

        /// <summary>
        /// Creates and cleares temp directories
        /// </summary>
        public static void InitializeTempDirectories()
        {
            CreateTempDirectories();
            ClearTempDirectoriesContent();
        }

        private static void CreateTempDirectories()
        {
            Directory.CreateDirectory(TempFolderPath);
            Directory.CreateDirectory(Path.Combine(TempFolderPath, "UndoStack"));
            Directory.CreateDirectory(Path.Combine(TempFolderPath, "RedoStack"));
        }

        public static void ClearTempDirectoriesContent()
        {
            ClearDirectory(RedoStackPath);
            ClearDirectory(UndoStackPath);
        }
    }
}
