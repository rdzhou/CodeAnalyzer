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
using System.Xml;

namespace CodeAnalysis
{
    class RelationDisplay
    {
        public static void Write(List<Repository> repos)
        {
            foreach(Repository repo in repos)
            {
                Console.Write("file: {0}\n", repo.file);
                Console.Write("----------------------------\n\n");
                foreach(Relation r in repo.relations)
                {
                    Console.Write("{0} {1} {2} {3} {4}\n", r.source.type, r.source.name, r.relation_type, r.target.type, r.target.name);
                }
                Console.Write("\n\n");
            }
        }
    }

    class RelationDisplayXML
    {
        public static void WriteRep(Repository rep, XmlDocument xml, XmlElement file)
        {
            foreach (Relation r in rep.relations)
            {
                XmlElement relation = xml.CreateElement("relation");
                relation.SetAttribute("relation_type", r.relation_type);
                file.AppendChild(relation);
                XmlElement source = xml.CreateElement("source");
                source.SetAttribute("type", r.source.type);
                source.SetAttribute("name", r.source.name);
                XmlElement target = xml.CreateElement("target");
                target.SetAttribute("type", r.target.type);
                target.SetAttribute("name", r.target.name);
                relation.AppendChild(source);
                relation.AppendChild(target);
            }
        }

        public static void Write(List<Repository> repos)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement root = xml.CreateElement("codeanalysis");
            xml.AppendChild(root);
            foreach (Repository rep in repos)
            {
                XmlElement file = xml.CreateElement("file");
                file.SetAttribute("path", rep.file);
                root.AppendChild(file);
                WriteRep(rep, xml, file);
            }
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter("codeanalysis_relation.xml", null);
                writer.Formatting = Formatting.Indented;
                xml.WriteTo(writer);
                writer.Close();
            }
            catch (XmlException xmlexp)
            {
                Console.WriteLine(xmlexp.Message);
            }
        }
    }
}
