using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Core
{
    public class Mmd2vgmArgs
    {
        public string srcFn;
        public string desFn;
        public string stPath;
        public Action<string> Disp;
        public bool isLoopEx;
        public int rendSecond;
        public ILog log;
        public IFile file;
    }
}
