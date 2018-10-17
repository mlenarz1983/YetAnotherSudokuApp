using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace YetAnotherSudokuPlayer.WinForms
{
    public static class Utils
    {
        public static T XmlDeserializeObject<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StringReader sr = new StringReader(xml))
            {
                return (T)xs.Deserialize(sr);
            }
        }
        public static string XmlSerializeObject(object obj)
        {
            string result = "";
            XmlSerializer xs = new XmlSerializer(obj.GetType());
            using (StringWriter sw = new StringWriter())
            {
                xs.Serialize(sw, obj);
                result = sw.ToString();
            }
            return result;
        }
    }
}
