using System;
using FileHelpers;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace MotoDetector.Classes
{

    [DelimitedRecord(";")]
    [IgnoreFirst()]
    public class Moto
    {

        public string ID = "";
        public string Fabricante = "";
        public string Modelo = "";
        public string Ciclo = "";
        public string Volume = "";
        public string Potencia = "";
        public string Peso = "";
        public string Peso_Pot = "";
        public string Altura = "";
        public string Largura = "";
        public string Comprimento = "";
        public string Altura_Assento = "";

    }

    public class Motos
    {

        public static Dictionary<string, Moto> Get()
        {
            Moto[] list;
            var assembly = typeof(Moto).GetTypeInfo().Assembly;
            var engine = new FileHelpers.FileHelperEngine<Moto>();
            using (var stream = assembly.GetManifestResourceStream("MotoDetector.Data.motos.csv"))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    list = engine.ReadStream(reader);
                }
            }
            return list.ToDictionary(x => x.ID);
        }
    }
}
