using System;
using System.Reflection;
using System.Text;
using System.IO;

namespace Core
{
    public interface ILog
    {
        string Path { get; set; }
        bool Debug { get; set; }
        StreamWriter Writer{ get; set; }

        void ForcedWrite(string msg);
        void ForcedWrite(Exception e);
        void Write(string msg);
        void Open();
        void Close();
    }
}
