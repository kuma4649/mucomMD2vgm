using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IFile
    {
        string Combine(string v1, string v2);
        string Combine(string path, string v1, string v2);
        void Delete(string tempPath);
        bool Exists(string srcFn);
        string GetDirectoryName(string v);
        string GetExtension(string desFn);
        string GetFileName(string fn);
        string GetFileNameWithoutExtension(string desFn);
        string GetFullPath(string srcFn);
        byte[] ReadAllBytes(string destPath);
        string[] ReadAllLines(string srcFn, Encoding encoding);
        void WriteAllBytes(string desFn, byte[] desBuf);
    }
}
