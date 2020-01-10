using System;
using FileHelpers;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace MotoDetector.Classes
{

    [DelimitedRecord(";")]
    public class ResNet50Feature
    {

        public string ID = "";
        public string Descricao = "";

    }

    public class ResNet50Features
    {

        public static Dictionary<string, ResNet50Feature> Get()
        {
            ResNet50Feature[] list;
            var assembly = typeof(ResNet50Feature).GetTypeInfo().Assembly;
            var engine = new FileHelpers.FileHelperEngine<ResNet50Feature>();
            using (var stream = assembly.GetManifestResourceStream("MotoDetector.Data.features.csv"))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    list = engine.ReadStream(reader);
                }
            }
            return list.ToDictionary(x => x.ID.ToLower());
        }
    }
}
