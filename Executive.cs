///////////////////////////////////////////////////////////////////////
// Executive.cs - Executive manages the control flow of Code Analyzer//
// ver 1.0                                                           //
// Language:    C#, 2013, .Net Framework 4.5                         //
// Platform:    Mac Air 2013, Win8.1                                 //
// Application: Project #1 for CSE681, Fall 2014                     //
// Author:      Rundong Zhou, 234944570, Syracuse University         //
//              (315) 696-1898, rzhou02@syr.edu                      //
///////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    class Argument
    {
        public bool bS, bR, bX;
        public string rootdir;
        public List<string> filePatterns;
        public Argument()
        {
            bS = bR = bX = false;
            rootdir = "";
            filePatterns = new List<string>();
        }
    }

    class Executive
    {
        static void Main(string[] args)
        {
            // Process comand line
            Argument argu = CMDParser.ParseCommand(args);

            // get all file reference
            FileMgr filemgr = new FileMgr();
            List<string> files = filemgr.GetFiles(argu);

            // Parse the file one by one
            foreach(string file in files)
            {
                XLog.LogLine(file);
                Parser parser = new Parser(file);
                if (!parser.Parse())
                {
                    XLog.LogLine("The file is invalid. {0}", file);
                }
                else
                {
                    // display the final results
                    Display.Write(argu.bX, argu.bR);
                }
            }

            Console.ReadLine();

        }
    }
}
