///////////////////////////////////////////////////////////////////////
// Display.cs - Display takes display command                        //
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
    class Display
    {
        public static void Write(List<Repository> repos, bool bX, bool bR)
        {
            // TODO: Write the final results to standard output
            // and XML file
            if(!bR)
            {
                if(bX)
                {
                    BasicDisplayXML.Write(repos);
                }
                else
                {
                    BasicDisplay.Write(repos);
                }
            }
            else
            {
                RelationDisplay.Write(repos);
            }
        }
    }
}
