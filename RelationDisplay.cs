///////////////////////////////////////////////////////////////////////
// RelationDisplay.cs - RelationDisplay output relation results      //
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
    class RelationDisplay
    {
        public static void Write(List<Repository> repos)
        {
            foreach(Repository repo in repos)
            {
                foreach(Relation r in repo.relations)
                {
                    Console.Write("{0} {1} {2} {3} {4}\n", r.source.type, r.source.name, r.relation_type, r.target.type, r.target.name);
                }
            }
        }
    }
}
