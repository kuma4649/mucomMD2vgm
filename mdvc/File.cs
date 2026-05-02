using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace mdvc
{
    public class File : IFile
    {
        public string Combine(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public string Combine(string path, string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public void Delete(string tempPath)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string srcFn)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryName(object v)
        {
            throw new NotImplementedException();
        }

        public string GetExtension(string desFn)
        {
            throw new NotImplementedException();
        }

        public string GetFileName(string fn)
        {
            throw new NotImplementedException();
        }

        public string GetFileNameWithoutExtension(string desFn)
        {
            throw new NotImplementedException();
        }

        public string GetFullPath(string srcFn)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string destPath)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string srcFn, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string includeFn)
        {
            throw new NotImplementedException();
        }

        public void WriteAllBytes(string desFn, byte[] desBuf)
        {
            throw new NotImplementedException();
        }
    }
}
