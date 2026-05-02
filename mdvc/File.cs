using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace mdvc
{
    public class File : IFile
    {
        public string Combine(string v1, string v2)
        {
            return Path.Combine(v1, v2);
        }

        public string Combine(string path, string v1, string v2)
        {
            return Path.Combine(path, v1, v2);
        }

        public void Delete(string tempPath)
        {
            System.IO.File.Delete(tempPath);
        }

        public bool Exists(string srcFn)
        {
            return System.IO.File.Exists(srcFn);
        }

        public string GetDirectoryName(string v)
        {
            return System.IO.Path.GetDirectoryName(v);
        }

        public string GetExtension(string desFn)
        {
            return System.IO.Path.GetExtension(desFn);
        }

        public string GetFileName(string fn)
        {
            return System.IO.Path.GetFileName(fn);
        }

        public string GetFileNameWithoutExtension(string desFn)
        {
            return System.IO.Path.GetFileNameWithoutExtension(desFn);
        }

        public string GetFullPath(string srcFn)
        {
            return System.IO.Path.GetFullPath(srcFn);
        }

        public byte[] ReadAllBytes(string destPath)
        {
            return System.IO.File.ReadAllBytes(destPath);
        }

        public string[] ReadAllLines(string srcFn, Encoding encoding)
        {
            return System.IO.File.ReadAllLines(srcFn, encoding);
        }

        public void WriteAllBytes(string desFn, byte[] desBuf)
        {
            System.IO.File.WriteAllBytes(desFn, desBuf);
        }
    }
}
