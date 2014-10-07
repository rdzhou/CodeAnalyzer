///////////////////////////////////////////////////////////////////////
// BasicDisplay.cs - BasicDisplay output basic results               //
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
    class BasicDisplay
    {
        public static void TraverseTree(Elem elem, int level)
        {
            string tabs = "";
            for (int i = 0; i < level; ++i)
            {
                tabs += "    ";
            }
            if(elem.type == "function")
            {
                Console.Write("{0}{1,-8}  {2}  [{3},{4}]\n", tabs,  
                    elem.type, elem.name, elem.end - elem.begin + 1,  elem.complexity);
            }
            else
            {
                Console.Write("{0}{1,-8}  {2}  ({3},{4})\n", tabs, elem.type, elem.name, elem.begin, elem.end);
            }
            
            foreach (Elem e in elem.children)
            {
                TraverseTree(e, level + 1);
            }
        }

        public static void WriteRep(Repository rep)
        {
            List<Elem> tree = rep.tree_locations;
            foreach (Elem e in tree)
            {
                TraverseTree(e, 0);
            }
        }

        public static void WriteSummary(List<Repository> repos)
        {
            int classNum, structNum, interfaceNum, enumNum, delegateNum, functionNum, maxComplexity, maxSize;
            classNum = structNum = interfaceNum = enumNum = delegateNum = functionNum = maxComplexity = maxSize = 0;
            foreach(Repository rep in repos)
            {
                foreach(Elem elem in rep.locations)
                {
                    if(elem.type == "class")
                        classNum++;
                    else if(elem.type == "sturct")
                        structNum++;
                    else if(elem.type == "interface")
                        interfaceNum++;
                    else if(elem.type == "enum")
                        enumNum++;
                    else if(elem.type == "delegate")
                        delegateNum++;
                    else if(elem.type == "function")
                    {
                        functionNum++;
                        if(elem.complexity > maxComplexity)
                        {
                            maxComplexity = elem.complexity;
                        }
                        if(elem.end - elem.begin + 1 > maxSize)
                        {
                            maxSize = elem.end - elem.begin + 1;
                        }
                    }
                }
            }
            Console.Write("SUMMARY:\n");
            Console.Write("----------------------------\n\n");
            Console.Write("{0,-20}: {1}\n", "FileNum", repos.Count);
            Console.Write("{0,-20}: {1}\n", "ClassNum", classNum);
            Console.Write("{0,-20}: {1}\n", "InterfaceNum", interfaceNum);
            Console.Write("{0,-20}: {1}\n", "StructNum", structNum);
            Console.Write("{0,-20}: {1}\n", "EnumNum", enumNum);
            Console.Write("{0,-20}: {1}\n", "DelegateNum", delegateNum);
            Console.Write("{0,-20}: {1}\n", "FunctionNum", functionNum);
            Console.Write("{0,-20}: {1}\n", "MaxFuncComplexity", maxComplexity);
            Console.Write("{0,-20}: {1}\n", "MaxFuncSize", maxSize);
        }

        public static void Write(List<Repository> repos)
        {
            foreach(Repository rep in repos)
            {
                Console.Write("file: {0}\n", rep.file);
                Console.Write("----------------------------\n\n");
                Console.Write("info inside braces:\n");
                Console.Write("\t for namespaces and types (begin, end)\n");
                Console.Write("\t for functions [size, complexity]\n\n");
                WriteRep(rep);
                Console.Write("\n\n");
            }
            WriteSummary(repos);
        }
    }

    class BasicDisplayXML
    {
        public static void TraverseTree(Elem elem, XmlDocument xml, XmlElement xml_elem)
        {
            XmlElement xml_node = xml.CreateElement(elem.type);
            xml_node.SetAttribute("name", elem.name);
            xml_elem.AppendChild(xml_node);
            if (elem.type == "function")
            {
                xml_node.SetAttribute("size", (elem.end - elem.begin + 1).ToString());
                xml_node.SetAttribute("complexity", (elem.complexity).ToString());
            }
            else
            {
                xml_node.SetAttribute("begin", (elem.begin).ToString());
                xml_node.SetAttribute("end", (elem.end).ToString());
            }

            foreach (Elem e in elem.children)
            {
                TraverseTree(e, xml, xml_node);
            }
        }

        public static void WriteRep(Repository rep, XmlDocument xml, XmlElement file)
        {
            List<Elem> tree = rep.tree_locations;
            foreach (Elem e in tree)
            {
                TraverseTree(e, xml, file);
            }
        }

        public static void CreateAndAppend(XmlDocument xml, XmlElement summary, string name, int val)
        {
            XmlElement node = xml.CreateElement(name);
            node.InnerText = val.ToString();
            summary.AppendChild(node);
        }

        public static void WriteSummary(List<Repository> repos, XmlDocument xml, XmlElement root)
        {
            int classNum, structNum, interfaceNum, enumNum, delegateNum, functionNum, maxComplexity, maxSize;
            classNum = structNum = interfaceNum = enumNum = delegateNum = functionNum = maxComplexity = maxSize = 0;
            foreach (Repository rep in repos)
            {
                foreach (Elem elem in rep.locations)
                {
                    if (elem.type == "class")
                        classNum++;
                    else if (elem.type == "sturct")
                        structNum++;
                    else if (elem.type == "interface")
                        interfaceNum++;
                    else if (elem.type == "enum")
                        enumNum++;
                    else if (elem.type == "delegate")
                        delegateNum++;
                    else if (elem.type == "function")
                    {
                        functionNum++;
                        if (elem.complexity > maxComplexity)
                        {
                            maxComplexity = elem.complexity;
                        }
                        if (elem.end - elem.begin + 1 > maxSize)
                        {
                            maxSize = elem.end - elem.begin + 1;
                        }
                    }
                }
            }
            XmlElement summary = xml.CreateElement("Summary");
            root.AppendChild(summary);
            CreateAndAppend(xml, summary, "FileNum", repos.Count);
            CreateAndAppend(xml, summary, "ClassNum", classNum);
            CreateAndAppend(xml, summary, "InterfaceNum", interfaceNum);
            CreateAndAppend(xml, summary, "StructNum", structNum);
            CreateAndAppend(xml, summary, "EnumNum", enumNum);
            CreateAndAppend(xml, summary, "DelegateNum", delegateNum);
            CreateAndAppend(xml, summary, "FunctionNum", functionNum);
            CreateAndAppend(xml, summary, "MaxFuncComplexity", maxComplexity);
            CreateAndAppend(xml, summary, "MaxFuncSize", maxSize);
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
                //Console.Write("file: {0}\n", rep.file);
                //Console.Write("----------------------------\n\n");
                //Console.Write("info inside braces:\n");
                //Console.Write("\t for namespaces and types (begin, end)\n");
                //Console.Write("\t for functions [size, complexity]\n\n");
                WriteRep(rep, xml, file);
                //Console.Write("\n\n");
            }
            WriteSummary(repos, xml, root);
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter("codeanalysis.xml", null);
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
