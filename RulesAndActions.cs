﻿///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int complexity { get; set; }

        public List<Elem> children_ = new List<Elem>();
        public List<Elem> children
        {
            get { return children_; }
        }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
            temp.Append("}");
            return temp.ToString();
        }
    }

    public class Repository
    {
        string file_;
        
        public string file
        {
            get
            {
                return file_;
            }
            set
            {
                file_ = value;
                foreach(Relation relation in relations_)
                {
                    relation.source.file = value;
                }
            }
        
        }
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        List<Elem> tree_locations_ = new List<Elem>();
        List<Relation> relations_ = new List<Relation>();
        public TypeTable type_table { get; set; }

        static Repository instance;

        public Repository()
        {
            instance = this;
        }

        public bool NotInFunction()
        {
            for(int i=0; i < stack_.count; ++i)
            {
                if(stack[i].type == "function")
                {
                    return false;
                }
            }
            return true;
        }

        public Elem TopClass()
        {
            for(int i= stack_.count-1; i>=0; --i)
            {
                if(stack_[i].type == "class" ||
                    stack_[i].type == "struct" ||
                    stack_[i].type == "interface")
                {
                    return stack_[i];
                }
            }
            return null;
        }

        public void AddRelation(Relation r)
        {
            foreach(Relation relation in relations_)
            {
                if(r.source.type == relation.source.type
                    && r.source.name == relation.source.name
                    && r.relation_type == relation.relation_type
                    && r.target.type == relation.target.type
                    && r.target.name == relation.target.name)
                {
                    return;
                }
            }
            relations_.Add(r);
        }

        public static Repository getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp

        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }

        public int curComplexity
        {
            get;
            set;
        }

        public int maxComplexity
        {
            get;
            set;
        }

        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
        }

        public List<Elem> tree_locations
        {
            get { return tree_locations_; }
        }

        public List<Relation> relations
        {
            get { return relations_;  }
        }
    }
    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository repo_;

        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            repo_.stack.push(elem);
            if (elem.type == "control" || elem.name == "anonymous")
                return;
            repo_.locations.Add(elem);
            if (repo_.stack.count <= 1)
            {
                repo_.tree_locations.Add(elem);
            }
            else
            {
                Elem top = repo_.stack[repo_.stack.count - 2];
                top.children.Add(elem);
            }

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (AAction.displayStack)
                repo_.stack.display();
        }
    }
    /////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope

    public class PopStack : AAction
    {
        Repository repo_;

        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).end == 0)
                            {
                                (repo_.locations[i]).end = repo_.semi.lineCount;
                                if(elem.type == "function")
                                {
                                    elem.complexity = repo_.maxComplexity;
                                    // Console.WriteLine("set complexity {0}", elem.complexity);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            local.Add(elem.type).Add(elem.name);
            if (local[0] == "control")
                return;

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }

    public class InitFnComplexity : AAction
    {
        Repository repo_;

        public InitFnComplexity(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            repo_.curComplexity = repo_.maxComplexity = 1;
            // Console.WriteLine("InitComplexity {0}, {1}", repo_.curComplexity, repo_.maxComplexity);
        }
    }

    //public class AddType : AAction
    //{
    //    Repository repo_;

    //    public AddType(Repository repo)
    //    {
    //        repo_ = repo;
    //    }
    //    public override void doAction(CSsemi.CSemiExp semi)
    //    {
    //        Elem elem = new Elem();
    //        elem.type = semi[0];  // expects type
    //        elem.name = semi[1];  // expects name
    //        elem.begin = repo_.semi.lineCount - 1;
    //        elem.end = 0;
    //        repo_.locations.Add(elem);
    //    }

    //}

    public class UpdateFnComplexityEnter : AAction
    {
        Repository repo_;

        public UpdateFnComplexityEnter(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            repo_.curComplexity++;
            if(repo_.curComplexity > repo_.maxComplexity)
            {
                repo_.maxComplexity = repo_.curComplexity;
            }
            // Console.WriteLine("UpdateFnComplexityEnter {0}, {1}", repo_.curComplexity, repo_.maxComplexity);
        }
    }

    public class UpdateFnComplexityLeave : AAction
    {
        Repository repo_;

        public UpdateFnComplexityLeave(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            repo_.curComplexity--;
            // Console.WriteLine("UpdateFnComplexityLeave {0}, {1}", repo_.curComplexity, repo_.maxComplexity);
        }
    }

    public class UpdateFnComplexityBraceless : AAction
    {
        Repository repo_;

        public UpdateFnComplexityBraceless(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            if(repo_.curComplexity + 1 > repo_.maxComplexity)
            {
                repo_.maxComplexity = repo_.curComplexity + 1;
            }
            // Console.WriteLine("UpdateFnComplexityBraceless {0}, {1}", repo_.curComplexity, repo_.maxComplexity);
        }
    }
    ///////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        Repository repo_;

        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }
        public override void display(CSsemi.CSemiExp semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.count; ++i)
                if (semi[i] != "\n" && !semi.isComment(semi[i]))
                    Console.Write("{0} ", semi[i]);
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }
    }
    /////////////////////////////////////////////////////////
    // concrete printing action, useful for debugging

    public class Print : AAction
    {
        Repository repo_;

        public Print(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }
    }
    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] != "{")
                return false;

            int index = semi.FindFirst("(");
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add("function").Add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    public class DetectDelegate : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("delegate");
            int indexLeftParenthesis = semi.Contains("(");

            if (index != -1 && indexLeftParenthesis != -1 && indexLeftParenthesis > 0)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("delegate").Add(semi[indexLeftParenthesis - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    public class DetectEnum : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("enum");
            // Console.WriteLine(semi.displayStr());

            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("enum").Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }


    /////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    public class DetectAnonymousScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("{");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("control").Add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("}");
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }

    public class DetectBracelessScope : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.FindFirst("(");
            if (index != -1 && -1 == semi.FindFirst("{"))
            {
                if(isSpecialToken(semi[index-1]) 
                    && 
                    // avoid #if
                    (index-2<0 || semi[index-2]!="#"))
                {
                    doActions(semi);
                    return true;
                }
            }
            return false;
        }
    }
    public class BuildCodeAnalyzer
    {
        protected Repository repo = new Repository();

        public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // this is default so redundant

            

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);
            PopStack pop = new PopStack(repo);



            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture function info
            DetectFunction detectFN = new DetectFunction();
            InitFnComplexity initFnComp = new InitFnComplexity(repo);
            detectFN.add(push);
            detectFN.add(initFnComp);
            parser.add(detectFN);


            // AddType addType = new AddType(repo);
            DetectDelegate detectDG = new DetectDelegate();
            // TODO: Add action for DetectDelegate
            // detectDG.add(push);
            // detectDG.add(addType);
            detectDG.add(push);
            detectDG.add(pop);
            parser.add(detectDG);

            UpdateFnComplexityEnter complexityEnter = new UpdateFnComplexityEnter(repo);

            DetectEnum detectEN = new DetectEnum();
            // TODO: Add action for DetectEnum
            // detectEN.add(push);
            detectEN.add(push);
            // detectEN.add(complexityEnter);
            // detectEN.add(addType);
            parser.add(detectEN);

            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            anon.add(complexityEnter);
            parser.add(anon);


            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            UpdateFnComplexityLeave complexityLeave = new UpdateFnComplexityLeave(repo);
            leave.add(pop);
            leave.add(complexityLeave);
            parser.add(leave);

            DetectBracelessScope braceless = new DetectBracelessScope();
            UpdateFnComplexityBraceless updateBraceless = new UpdateFnComplexityBraceless(repo);
            braceless.add(updateBraceless);
            parser.add(braceless);

            // parser configured
            return parser;
        }
    }
}

