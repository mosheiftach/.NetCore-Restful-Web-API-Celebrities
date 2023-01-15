using Newtonsoft.Json;
using System;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication2123123
{
    public class JsonFileUtils
    {
        private static readonly JsonSerializerOptions _options =
            new JsonSerializerOptions();

        public static void SimpleWrite(object obj, string fileName)
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(obj, _options);
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Celeb.json");
            File.WriteAllText(dataPath, jsonString);
        }

    }
}