using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NetworkExtensions
{
    public class CfgMan
    {
        private static readonly string CurrentDir = Directory.GetCurrentDirectory();

        public static void Serialize<T>(T cfg, string path)
        {
            using (FileStream fs = new FileStream(CurrentDir + @"\" + path, FileMode.Create))
            {
                XmlWriterSettings xmlSettings = new XmlWriterSettings { Indent = true };
                XmlWriter writer = XmlWriter.Create(fs, xmlSettings);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(writer, cfg);
            }
        }
        public static void Deserialize<T>(ref T cfg, string path)
        {
            if (!File.Exists(CurrentDir + @"\" + path))
                return;
           
            using (FileStream fs = new FileStream(CurrentDir + @"\" + path, FileMode.Open))
            {
                XmlReader reader = XmlReader.Create(fs);
                XmlSerializer ser = new XmlSerializer(typeof(T));
                cfg = (T)ser.Deserialize(reader);
            }
        }
    }
}
