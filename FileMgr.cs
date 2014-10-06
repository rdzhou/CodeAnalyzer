///////////////////////////////////////////////////////////////////////
// FileMgr.cs - FileMgr convert path and patterns to path list       //
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
using Navig;

namespace CodeAnalysis
{
    class FileMgr
    {
        private List<string> m_Files;

        public FileMgr()
        {
            m_Files = new List<string>();
        }
        public void OnFile(string file)
        {
            m_Files.Add(file);
        }

        public List<string> GetFiles(Argument argu)
        {
            // TODO: get all files
            Navigate navi = new Navigate();
            navi.newFile += new Navigate.newFileHandler(OnFile);
            foreach(string pattern in argu.filePatterns)
            {
              navi.go(argu.rootdir, pattern, argu.bS);
            }
            return m_Files;
        }
    }
}
