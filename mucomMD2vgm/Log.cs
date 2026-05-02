using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace mucomMD2vgm
{
    public class Log : Core.ILog
    {
        public string Path { get; set; } = "";
        public bool Debug { get; set; } = false;
        public StreamWriter Writer { get; set; }

        public void ForcedWrite(string msg)
        {
            if (Writer == null) return;
            try
            {
                if (Path == "")
                {
                    string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    fullPath = System.IO.Path.Combine(fullPath, "KumaApp", AssemblyTitle);
                    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                    Path = System.IO.Path.Combine(fullPath, "log.txt");
                    if (System.IO.File.Exists(Path)) System.IO.File.Delete(Path);
                }

                DateTime dtNow = DateTime.Now;
                string timefmt = dtNow.ToString("yyyy/MM/dd HH:mm:ss\t");

                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                Writer.WriteLine(timefmt + msg);
            }
            catch
            {
            }
        }

        public void ForcedWrite(Exception e)
        {
            if (Writer == null) return;
            try
            {
                if (Path == "")
                {
                    string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    fullPath = System.IO.Path.Combine(fullPath, "KumaApp", AssemblyTitle);
                    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                    Path = System.IO.Path.Combine(fullPath, "log.txt");
                    if (System.IO.File.Exists(Path)) System.IO.File.Delete(Path);
                }

                DateTime dtNow = DateTime.Now;
                string timefmt = dtNow.ToString("yyyy/MM/dd HH:mm:ss\t");

                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                string msg = string.Format("例外発生:\r\n- Type ------\r\n{0}\r\n- Message ------\r\n{1}\r\n- Source ------\r\n{2}\r\n- StackTrace ------\r\n{3}\r\n", e.GetType().Name, e.Message, e.Source, e.StackTrace);
                Exception ie = e;
                while (ie.InnerException != null)
                {
                    ie = ie.InnerException;
                    msg += string.Format("内部例外:\r\n- Type ------\r\n{0}\r\n- Message ------\r\n{1}\r\n- Source ------\r\n{2}\r\n- StackTrace ------\r\n{3}\r\n", ie.GetType().Name, ie.Message, ie.Source, ie.StackTrace);
                }

                Writer.WriteLine(timefmt + msg);
                System.Console.WriteLine(msg);
            }
            catch
            {
            }
        }

        public void Write(string msg)
        {
            if (!Debug)
            {
                return;
            }

            if (Writer == null) return;

            try
            {
                if (Path == "")
                {
                    string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    fullPath = System.IO.Path.Combine(fullPath, "KumaApp", AssemblyTitle);
                    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                    Path = System.IO.Path.Combine(fullPath, "log.txt");
                    if (System.IO.File.Exists(Path)) System.IO.File.Delete(Path);
                }

                DateTime dtNow = DateTime.Now;
                string timefmt = dtNow.ToString("yyyy/MM/dd HH:mm:ss\t");

                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                Writer ??= new StreamWriter(Path, true, sjisEnc);
                Writer.WriteLine(timefmt + msg);
                Writer.Flush();
            }
            catch
            {
            }
        }

        public void Open()
        {
            try
            {
                if (Path == "")
                {
                    string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    fullPath = System.IO.Path.Combine(fullPath, "KumaApp", AssemblyTitle);
                    if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                    Path = System.IO.Path.Combine(fullPath, "log.txt");
                    if (System.IO.File.Exists(Path)) System.IO.File.Delete(Path);
                }
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
                Writer = new StreamWriter(Path, true, sjisEnc);
            }
            catch
            {
                Writer = null;
            }
        }

        public void Close()
        {
            Writer?.Close();
        }

        private string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.BaseDirectory);// Assembly.GetExecutingAssembly().CodeBase);
            }
        }
    }
}
