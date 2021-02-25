using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FistVR;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using TNHTweaker;
using TNHTweaker.ObjectTemplates;

namespace JSONBuilder
{
    public static class JSONBuilder
    {

        public static void Main(string[] args)
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            JSchema schema = generator.Generate(typeof(CustomCharacter));

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CustomCharacterSchema.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter file = File.CreateText(path))
            {
                using(JsonTextWriter writer = new JsonTextWriter(file))
                {
                    schema.WriteTo(writer);
                }
            }



            schema = generator.Generate(typeof(SosigTemplate));

            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SosigTemplate.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter file = File.CreateText(path))
            {
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    schema.WriteTo(writer);
                }
            }



        }
    }
}
