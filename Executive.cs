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
        static void Traverse(Elem elem, int level)
        {
            string tabs = "";
            for (int i = 0; i < level; ++i)
            {
                tabs += '\t';
            }
            Console.WriteLine(tabs + elem.type + " " + elem.name + " " + elem.begin + " " + elem.end);
            foreach (Elem e in elem.children)
            {
                Traverse(e, level + 1);
            }
        }

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

                XLog.LogLine("  Processing file {0}", file);

                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = true;
                if (!semi.open(file))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }

                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------\n");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                    Console.Write("\n\n  locations table contains:");
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                foreach (Elem e in table)
                {
                    Console.Write("\n  {0,10}, {1,25}, {2,5}, {3,5}", e.type, e.name, e.begin, e.end);
                    if (e.type == "function")
                        Console.Write("{0,5}", e.complexity);

                }
                Console.WriteLine();
                Console.Write("\n\n  That's all folks!\n\n");

                List<Elem> tree = rep.tree_locations;
                Console.WriteLine(tree.Count);
                foreach (Elem e in tree)
                {
                    Traverse(e, 0);
                }
                semi.close();
             

                // display the final results
                Display.Write(argu.bX, argu.bR);
            }

            Console.ReadLine();

        }
    }
}
