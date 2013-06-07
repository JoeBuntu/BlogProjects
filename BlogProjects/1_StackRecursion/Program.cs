using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
 
namespace StackRecursion
{
    // 06/05/2013 by Joe Cooper
    public sealed class Program
    { 
        public static void Main(string[] args)
        {
            //Example1();
            Example2();
        }

        #region Example1

        //Example 1
        private static void Example1()
        {
            for (int cycle = 0; cycle < 5; cycle++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                for (int i = 0; i < 5000; i++)
                {
                    StringBuilder buffer = new StringBuilder();
                    Assembly assembly = Assembly.Load("PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    foreach (Type t in assembly.GetExportedTypes())
                    {
                        PrintTypesUsingRecursion(t, buffer, 0);
                        //PrintTypesUsingStack(t, buffer, 0);
                    }
                    //Trace.WriteLine(buffer.ToString());
                }
                watch.Stop();

                Trace.WriteLine("Time Elapsed: " + watch.ElapsedMilliseconds.ToString());
            }
        }

        //Example 1 using recursive method
        private static void PrintTypesUsingRecursion(Type typeToPrint, StringBuilder buffer, Int32 indent)
        {
            //start printing element
            buffer.Append(' ', indent);
            buffer.Append('<').Append(typeToPrint.Name);

            if (typeToPrint.BaseType == null)
            {
                //this type has no base type, close it off
                buffer.AppendLine("/>");
                return;
            }
            else
            {
                //close off element
                buffer.AppendLine(">");

                //print nested children
                PrintTypesUsingRecursion(typeToPrint.BaseType, buffer, indent + 3);

                //closing element
                buffer.Append(' ', indent);
                buffer.Append("</").Append(typeToPrint.Name).AppendLine(">");
            }
        }

        //Example 1 using stack
        private static void PrintTypesUsingStack(Type typeToPrint, StringBuilder buffer, Int32 indent)
        {
            System.Type currentType = typeToPrint;
            Stack<Type> stack = new Stack<Type>();

            //walk up the hierarchy chain
            while (true)
            {
                //start printing element
                buffer.Append(' ', indent);
                buffer.Append('<').Append(currentType.Name);

                if (currentType.BaseType == null)
                {
                    //this type has no base type, close it off
                    buffer.AppendLine("/>");
                    break;
                }
                else
                {
                    //close off element
                    buffer.AppendLine(">");

                    //prepare for next level 
                    stack.Push(currentType);
                    currentType = currentType.BaseType;
                    indent += 3;
                }
            }

            //walk back down the hierarchy chain
            while (stack.Any())
            {
                currentType = stack.Pop();

                //closing element
                indent -= 3;
                buffer.Append(' ', indent);
                buffer.Append("</").Append(currentType.Name).AppendLine(">");
            }
        }

        #endregion

        #region Example2

        //Example 2
        private static void Example2()
        {
            //Run and time 5 sets of 50,0000 
            for (int cycle = 0; cycle < 5; cycle++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                for (int i = 0; i < 500000; i++)
                {
                    Random r = new Random(5);
                    Node n = new Node(null, 10);

                    AddChildrenToNodeUsingRecursiveMethods(r, n);
                    //AddChildrenToNodeUsingStack(r, n);
                    //AddChildrenToNodeUsingStackWithFrames(r, n);
                }
                watch.Stop();

                Trace.WriteLine("Time Elapsed: " + watch.ElapsedMilliseconds.ToString());
            }

            //for visualizing the tree
            /*
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create("Example2Data.xml", settings))
                {
                    DataContractSerializer ds = new DataContractSerializer(typeof(Node));
                    ds.WriteObject(writer, n);
                }
            */
        }

        //Example 2 - using recursive methods
        private static void AddChildrenToNodeUsingRecursiveMethods(Random r, Node n)
        {
            if (n.Value != 3)
            {
                for (int i = 0; i < n.Value; i++)
                {
                    Node child = new Node(n, r.Next(1, 4));
                    n.Children.Add(child);
                    AddChildrenToNodeUsingRecursiveMethods(r, child);
                }
            }
        }

        //Example 2 - using stack
        private static void AddChildrenToNodeUsingStack(Random r, Node n)
        {
            Node currentNode = null;
            Stack<Node> nodeStack = new Stack<Node>();
            nodeStack.Push(n);

            while (nodeStack.Any())
            {
                currentNode = nodeStack.Pop();
                if (currentNode.Value != 4 && currentNode.Children.Count < currentNode.Value)
                {
                    nodeStack.Push(currentNode);

                    Node child = new Node(currentNode, r.Next(1, 4));
                    currentNode.Children.Add(child);

                    nodeStack.Push(child);
                }
            }
        }

        private static void AddChildrenToNodeUsingStackWithFrames(Random r, Node n)
        {
            MyFrameData currentFrame;
            Stack<MyFrameData> nodeStack = new Stack<MyFrameData>();
            nodeStack.Push(new MyFrameData(n, n.Value, 0));

            while (nodeStack.Any())
            {
                currentFrame = nodeStack.Pop();
                if (currentFrame.Value != 3 && currentFrame.NumberChildren < currentFrame.Value)
                {
                    Node child = new Node(currentFrame.Node, r.Next(1, 4));
                    currentFrame.Node.Children.Add(child);

                    currentFrame.NumberChildren++;
                    nodeStack.Push(currentFrame);
                    nodeStack.Push(new MyFrameData(child, child.Value, 0));
                }
            }
        }

        public class MyFrameData
        {
            public Node Node { get; set; }
            public Int32 Value { get; set; }
            public Int32 NumberChildren { get; set; }

            public MyFrameData(Node node, Int32 value, Int32 numberChildren)
            {
                Node = node;
                Value = value;
                NumberChildren = numberChildren;
            }
        }

        [DataContract(IsReference = true)]
        public sealed class Node
        {
            public Node(Node parent, int value)
            {
                Parent = parent;
                Value = value;
            }

            [DataMember(Order = 0)]
            public Node Parent { get; set; }

            [DataMember(Order = 0)]
            public int Value { get; set; }

            [DataMember(Order = 1)]
            public List<Node> Children
            {
                get { return _Children; }
                set { _Children = value; }
            }
            private List<Node> _Children = new List<Node>();
        }

        #endregion

    } 
}
