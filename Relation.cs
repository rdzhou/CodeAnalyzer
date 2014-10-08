using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeAnalysis
{
    public class TypeNode
    {
        public string type { get; set; }
        public string name { get; set; }
        public string file { get; set; }
    }

    public class FileInfo
    {
        public string file { get; set; }
        List<TypeNode> list_ = new List<TypeNode>();
        public List<TypeNode> list
        {
            get { return list_; }
        }
    }

    public class Relation
    {
        public string relation_type { get; set; }
        public TypeNode source { get; set; }
        public TypeNode target { get; set; }
    }

    public class FindInheritance : AAction
    {
        Repository repo_;

        public FindInheritance(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains(":");
            if(index != -1)
            {
                TypeNode source = new TypeNode();
                source.type = semi[0];
                source.name = semi[1];
                for(int i=index; i< semi.count;++i)
                {
                    TypeNode target = repo_.type_table.Find(semi[i]);
                    if( target != null)
                    {
                        Relation relation = new Relation();
                        relation.source = source;
                        relation.target = target;
                        relation.relation_type = "inheritance";
                        repo_.AddRelation(relation);
                    }
                }
            }
        }
    }

    public class FindComposition : AAction
    {
        Repository repo_;

        public FindComposition(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            if(!repo_.NotInFunction())
            {
                return;
            }

            Elem topClass = repo_.TopClass();
            if(null == topClass)
            {
                return;
            }

            TypeNode source = new TypeNode();
            source.type = topClass.type;
            source.name = topClass.name;
            TypeNode target;
            for (int i = 0; i < semi.count; ++i)
            {
                target = repo_.type_table.Find(semi[i]);
                if (target != null && target.type == "struct")
                {
                    Relation relation = new Relation();
                    relation.source = source;
                    relation.target = target;
                    relation.relation_type = "composition";
                    repo_.AddRelation(relation);
                }
            }
        }
    }

    public class FindAggregation : AAction
    {
        Repository repo_;

        public FindAggregation(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("new");
            if(-1 == index)
            {
                return;
            }

            Elem topClass = repo_.TopClass();
            if (null == topClass)
            {
                return;
            }

            TypeNode source = new TypeNode();
            source.type = topClass.type;
            source.name = topClass.name;
            TypeNode target;
            for (int i = index+1; i < semi.count; ++i)
            {
                target = repo_.type_table.Find(semi[i]);
                if (target != null)
                {
                    Relation relation = new Relation();
                    relation.source = source;
                    relation.target = target;
                    relation.relation_type = "aggregation";
                    repo_.AddRelation(relation);
                }
            }
        }
    }

    public class FindLocal : AAction
    {
        Repository repo_;

        public FindLocal(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            if(repo_.NotInFunction())
            {
                return;
            }

            Elem topClass = repo_.TopClass();
            if (null == topClass)
            {
                return;
            }

            TypeNode source = new TypeNode();
            source.type = topClass.type;
            source.name = topClass.name;
            TypeNode target;
            for (int i = 0; i < semi.count; ++i)
            {
                target = repo_.type_table.Find(semi[i]);
                if (target != null && (target.type == "struct" || target.type == "enum"))
                {
                    Relation relation = new Relation();
                    relation.source = source;
                    relation.target = target;
                    relation.relation_type = "using";
                    repo_.AddRelation(relation);
                }
            }
        }
    }

    public class FindArg : AAction
    {
        Repository repo_;

        public FindArg(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem topClass = repo_.TopClass();
            if (null == topClass)
            {
                return;
            }

            TypeNode source = new TypeNode();
            source.type = topClass.type;
            source.name = topClass.name;
            TypeNode target;
            for (int i = 0; i < semi.count; ++i)
            {
                target = repo_.type_table.Find(semi[i]);
                if (target != null)
                {
                    Relation relation = new Relation();
                    relation.source = source;
                    relation.target = target;
                    relation.relation_type = "using";
                    repo_.AddRelation(relation);
                }
            }
        }
    }

    public class TypeTable
    {
        List<FileInfo> table_ = new List<FileInfo>();


        public TypeTable(List<Repository> repos)
        {
            foreach(Repository repo in repos)
            {
                FileInfo fileinfo = new FileInfo();
                fileinfo.file = repo.file;
                table_.Add(fileinfo);
                foreach(Elem e in repo.locations)
                {
                    if(e.type == "namespace" || e.type == "function")
                    {
                        continue;
                    }
                    TypeNode node = new TypeNode();
                    node.file = fileinfo.file;
                    node.type = e.type;
                    node.name = e.name;
                    fileinfo.list.Add(node);
                }
            }
        }

        public TypeNode Find(string name)
        {
            foreach(FileInfo fileinfo in table_)
            {
                foreach(TypeNode node in fileinfo.list)
                {
                    if(node.name == name)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

    }

    public class DetectClassRelation : ARule
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
                for (int i = index; i < semi.count; ++i )
                {
                    local.Add(semi[i]);
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }

    public class DetectFunctionRelation : ARule
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
                for (int i = index + 1; i < semi.count; ++i)
                {
                    local.Add(semi[i]);
                }
                doActions(local);
                return true;
            }
            return false;
        }
    }

    public class DetectOthers : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            doActions(semi);
            return true;
        }
    }

    public class BuildRelationAnalyzer : BuildCodeAnalyzer
    {

        public BuildRelationAnalyzer(CSsemi.CSemiExp semi, TypeTable table):base(semi)
        {
            repo.type_table = table;
        }
        
        public override Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // this is default so redundant

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);
            PopStack pop = new PopStack(repo);
            FindInheritance inheri = new FindInheritance(repo);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClassRelation detectCl = new DetectClassRelation();
            detectCl.add(push);
            detectCl.add(inheri);
            parser.add(detectCl);

            // capture function info
            DetectFunctionRelation detectFN = new DetectFunctionRelation();
            FindArg arg = new FindArg(repo);
            detectFN.add(push);
            detectFN.add(arg);
            parser.add(detectFN);

            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);

            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            leave.add(pop);
            parser.add(leave);

            DetectOthers others = new DetectOthers();
            FindComposition comp = new FindComposition(repo);
            FindAggregation aggr = new FindAggregation(repo);
            FindLocal local = new FindLocal(repo);
            others.add(comp);
            others.add(aggr);
            others.add(local);
            parser.add(others);


            // parser configured
            return parser;
        }
    }

    struct TestStruct
    {

    }

    class Testcomp
    {
        TestStruct t;
        public void TestLocal()
        {
            TestStruct local;
        }
    }
}