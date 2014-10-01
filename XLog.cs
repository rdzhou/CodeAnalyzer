using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class XLog
    {
        public static void LogLine(string content, params object[] arg)
        {
            Console.WriteLine(content, arg);
        }

        public static void Log(string content, params object[] arg)
        {
            Console.Write(content,arg);
        }
    }
}
