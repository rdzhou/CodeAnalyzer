///////////////////////////////////////////////////////////////////////
// CMDParser.cs - CMDParser parses command line                      //
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

namespace CodeAnalysis
{
    class CMDParser
    {
        public static Argument ParseCommand(string[] args)
        {
            Argument ret = new Argument();
            // TODO: parse the command line and fill ret

            bool bPathGot = false;
            foreach (string arg in args)
            {
                // if match /s
                if(arg.Equals("/s", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.bS = true;
                }
                // if match /r
                else if (arg.Equals("/r", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.bR = true;
                }
                // if match /x
                else if (arg.Equals("/x", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret.bX = true;
                } 
                else if(!bPathGot)
                {
                    ret.rootdir = arg;
                    bPathGot = true;
                }
                else
                {
                    ret.filePatterns.Add(arg);
                }
            }

            XLog.LogLine("S {0}, R {1}, X {2}", ret.bS, ret.bR, ret.bX);
            XLog.LogLine("rootdir " + ret.rootdir);
            XLog.LogLine("patterns:");
            foreach (string arg in ret.filePatterns)
            {
                XLog.LogLine(arg);
            }
            XLog.LogLine("end");

            //try
            //{
            //    // parse command line argument into path and file name
            //    string path, filePattern;
            //    demo.splitArg(arg, out path, out filePattern);
            //    Console.Write(
            //      "\n  path = {0}\n  file pattern = {1}\n", path, filePattern
            //    );

            //    // find files on path matching pattern

            //    Console.Write("\n  Files found on path, matching pattern");
            //    string[] files = Directory.GetFiles(path, filePattern);

            //    foreach (string file in files)
            //    {
            //        Console.Write("\n    " + Path.GetFileName(file));
            //        if (Path.GetExtension(file) == ".cs")
            //            LastCSharpFile = file;
            //    }
            //}
            //catch (Exception except)
            //{
            //    Console.Write("\n  error in command line argument: {0}", arg);
            //    Console.Write("\n  {0}\n\n", except.Message);
            //}


            return ret;
        }
    }
}
