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

namespace CodeAnalysis
{
    public class Argument
    {
        public bool bS { get; set; }
        public bool bR { get; set; }
        public bool bX { get; set; }
        public string rootdir { get; set; }
        public List<string> filePatterns_ = new List<string>();

        public List<string> filePatterns
        {
            get { return filePatterns_;  }
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

            List<Repository> repos = new List<Repository>();

            // Parse the file one by one
            foreach(string file in files)
            {
                // XLog.LogLine(file);

                // XLog.LogLine("  Processing file {0}", file);

                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = true;
                if (!semi.open(file))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                rep.file = file;
                repos.Add(rep);
                semi.close();
            }

            if(argu.bR)
            {
                TypeTable table = new TypeTable(repos);
                repos = new List<Repository>();
                foreach (string file in files)
                {
                    CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                    semi.displayNewLines = true;
                    if (!semi.open(file))
                    {
                        Console.Write("\n  Can't open {0}\n\n", args[0]);
                        return;
                    }
                    BuildRelationAnalyzer builder = new BuildRelationAnalyzer(semi, table);
                    Parser parser = builder.build();

                    try
                    {
                        while (semi.getSemi())
                            parser.parse(semi);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n\n  {0}\n", ex.Message);
                    }
                    Repository rep = Repository.getInstance();
                    rep.file = file;
                    repos.Add(rep);
                    semi.close();
                }
            }

            // display the final results
            Display.Write(repos, argu.bX, argu.bR);

            Console.ReadLine();

        }
    }
}
