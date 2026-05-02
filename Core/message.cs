using System.Collections.Generic;
using System.Reflection;
//using System.IO;

namespace Core
{
    public class Msg
    {

        private static Dictionary<string, string> dicMsg = new Dictionary<string, string>();

        public static void Init(IFile file)
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = file.GetDirectoryName(myAssembly.Location);
            string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
            string filename = file.Combine(path, "lang", string.Format("message.{0}.txt", lang));
            string[] lines = null;
            try
            {
                if (file.Exists(filename))
                    lines = file.ReadAllLines(filename,Common.myenc);
                else
                    lines = file.ReadAllLines(file.Combine(path, "lang", "message.txt"),Common.myenc);
            }
            catch
            {

            }

            if (lines != null)
            {
                foreach (string line in lines)
                {
                    try
                    {
                        if (line == null) continue;
                        if (line == "") continue;
                        string str = line.Trim();
                        if (str == "") continue;
                        if (str[0] == ';') continue;
                        string code = str.Substring(0, str.IndexOf("=")).Trim();
                        string msg = str.Substring(str.IndexOf("=") + 1, str.Length - str.IndexOf("=") - 1);
                        if (dicMsg.ContainsKey(code)) continue;

                        dicMsg.Add(code, msg);
                    }
                    catch { }
                }
            }
        }

        public static string get(string code)
        {
            if (dicMsg.ContainsKey(code))
            {
                return dicMsg[code].Replace("\\r", "\r").Replace("\\n", "\n");
            }
            return "<no message>";
        }

    }
}
