using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

//this code demonstrates how to build a context class using thread local storage and also shows
//how TLS uniqueness is determined by thread & app domain
namespace _2_TLSContext
{
    //derive from MarshalByRefObject so a proxy can be used to marshal this across app domains
    public class Program : MarshalByRefObject
    { 
        static void Main(string[] args)
        {
            Example1();
            Example2();
            Example3(); 
 
            Console.ReadLine();
        }

        //single app domain
        private static void Example1()
        {
            //run this example in another AppDomain so that it is not polluting the TLS
            //of the current AppDomain 
            AppDomain domain1 = AppDomain.CreateDomain("Domain-1");
            Program p1 = (Program)domain1.CreateInstanceFromAndUnwrap("2_TLSContext.exe", "_2_TLSContext.Program");

            //output will vary by machine
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(1));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(2));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(3));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(4));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(5));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(6));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(7));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(8));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(9));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(10));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(10));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(11));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(12));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(13));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(14));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(15));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(16));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(17));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(18));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(19));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(20));

            /* Output:
            Creating new context. Thread: 7 AppDomain: Domain-1
            Creating new context. Thread: 14 AppDomain: Domain-1
            Creating new context. Thread: 15 AppDomain: Domain-1
            Thread 14 AppDomain: Domain-1                  Prev Value:  0 New Value:  4
            Thread 15 AppDomain: Domain-1                  Prev Value:  0 New Value:  3
            Creating new context. Thread: 6 AppDomain: Domain-1
            Thread 14 AppDomain: Domain-1                  Prev Value:  4 New Value:  5
            Thread 15 AppDomain: Domain-1                  Prev Value:  3 New Value:  6
            Thread  6 AppDomain: Domain-1                  Prev Value:  0 New Value:  1
            Thread 14 AppDomain: Domain-1                  Prev Value:  5 New Value:  7
            Thread 15 AppDomain: Domain-1                  Prev Value:  6 New Value:  8
            Thread  6 AppDomain: Domain-1                  Prev Value:  1 New Value:  9
            Thread  7 AppDomain: Domain-1                  Prev Value:  0 New Value:  2
            Thread 14 AppDomain: Domain-1                  Prev Value:  7 New Value: 10
            Thread 15 AppDomain: Domain-1                  Prev Value:  8 New Value: 10
            Thread  7 AppDomain: Domain-1                  Prev Value:  2 New Value: 12
            Thread 14 AppDomain: Domain-1                  Prev Value: 10 New Value: 13
            Thread 15 AppDomain: Domain-1                  Prev Value: 10 New Value: 14
            Thread  6 AppDomain: Domain-1                  Prev Value:  9 New Value: 11
            Thread  7 AppDomain: Domain-1                  Prev Value: 12 New Value: 15
            Thread 14 AppDomain: Domain-1                  Prev Value: 13 New Value: 16
            Thread  7 AppDomain: Domain-1                  Prev Value: 15 New Value: 19
            Thread  6 AppDomain: Domain-1                  Prev Value: 11 New Value: 18
            Thread 15 AppDomain: Domain-1                  Prev Value: 14 New Value: 17
            Thread 14 AppDomain: Domain-1                  Prev Value: 16 New Value: 20*/
        }

        //multiple app domains
        private static void Example2()
        {
            //create instance for running threads in the current AppDomain
            AppDomain domain1 = AppDomain.CreateDomain("Domain-1");
            AppDomain domain2 = AppDomain.CreateDomain("Domain-2");
            Program p1 = (Program)domain1.CreateInstanceFromAndUnwrap("2_TLSContext.exe", "_2_TLSContext.Program");
            Program p2 = (Program)domain2.CreateInstanceFromAndUnwrap("2_TLSContext.exe", "_2_TLSContext.Program");
 
            //start on same domain...
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(1));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(2));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(3));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(4));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(5));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(6));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(7));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(8));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(9));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(10));

            //now demonstrate 2nd AppDomain uniqueness
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(10));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(11));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(12));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(13));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(14));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(15));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(16));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(17));
            ThreadPool.QueueUserWorkItem((x) => p2.SomeMethod(18));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(19));
            ThreadPool.QueueUserWorkItem((x) => p1.SomeMethod(20));

            /* Output
            Creating new context. Thread: 12 AppDomain: Domain-1
            Creating new context. Thread: 14 AppDomain: Domain-1
            Creating new context. Thread: 13 AppDomain: Domain-1
            Thread 14 AppDomain: Domain-1                  Prev Value:  0 New Value:  4
            Thread 12 AppDomain: Domain-1                  Prev Value:  0 New Value:  3
            Thread 14 AppDomain: Domain-1                  Prev Value:  4 New Value:  5
            Thread 13 AppDomain: Domain-1                  Prev Value:  0 New Value:  1
            Creating new context. Thread: 11 AppDomain: Domain-1
            Thread 11 AppDomain: Domain-1                  Prev Value:  0 New Value:  2
            Thread 12 AppDomain: Domain-1                  Prev Value:  3 New Value:  6
            Thread 14 AppDomain: Domain-1                  Prev Value:  5 New Value:  7
            Thread 13 AppDomain: Domain-1                  Prev Value:  1 New Value:  8
            Thread 11 AppDomain: Domain-1                  Prev Value:  2 New Value:  9
            Thread 12 AppDomain: Domain-1                  Prev Value:  6 New Value: 10
            Creating new context. Thread: 12 AppDomain: Domain-2
            Creating new context. Thread: 14 AppDomain: Domain-2
            Creating new context. Thread: 11 AppDomain: Domain-2
            Thread 14 AppDomain: Domain-2                  Prev Value:  0 New Value: 10
            Creating new context. Thread: 13 AppDomain: Domain-2
            Thread 11 AppDomain: Domain-2                  Prev Value:  0 New Value: 12
            Thread 14 AppDomain: Domain-2                  Prev Value: 10 New Value: 14
            Thread 11 AppDomain: Domain-2                  Prev Value: 12 New Value: 15
            Thread 13 AppDomain: Domain-2                  Prev Value:  0 New Value: 11
            Thread 12 AppDomain: Domain-2                  Prev Value:  0 New Value: 13
            Thread 14 AppDomain: Domain-2                  Prev Value: 14 New Value: 16
            Thread 13 AppDomain: Domain-2                  Prev Value: 11 New Value: 18
            Thread 11 AppDomain: Domain-2                  Prev Value: 15 New Value: 17
            Thread 12 AppDomain: Domain-1                  Prev Value: 10 New Value: 19
            Thread 14 AppDomain: Domain-1                  Prev Value:  7 New Value: 20 */
        }

        //TLS will not flow to other threads as CallContext will
        private static void Example3()
        {
            //run this example in another AppDomain so that it is not polluting the TLS
            //of the current AppDomain 
            AppDomain domain1 = AppDomain.CreateDomain("Domain-1");
            Program p1 = (Program)domain1.CreateInstanceFromAndUnwrap("2_TLSContext.exe", "_2_TLSContext.Program");

            //execute on this thread
            p1.SomeMethod(1);

            //will TLS be flowed to thread 2? Answer: No
            Thread t2 = new Thread((x) => p1.SomeMethod(2));
            t2.Start();
            t2.Join();
        }
 
        private void SomeMethod(Int32 value1)
        {
            //get previous value and set new
            Int32 prevValue = MyContext.Current.Value1;
            MyContext.Current.Value1 = value1;

            string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            string appDomain = AppDomain.CurrentDomain.FriendlyName;
            string currentValue = MyContext.Current.Value1.ToString();
            Trace.WriteLine(string.Format("Thread {0,2} AppDomain: {1,-25} Prev Value: {2,2} New Value: {3,2}", threadId, appDomain, prevValue.ToString(), currentValue));
        }


    }

    public class MyContext
    {
        //this will store a MyContext instance that is unique to each Thread & AppDomain
        private static ThreadLocal<MyContext> _Instance;

        //type constructor
        static MyContext()
        {
            //specify a value factory
            _Instance = new ThreadLocal<MyContext>(() =>
                {
                    //explicitly call ToString() so no boxing of value types
                    Trace.WriteLine(string.Format("Creating new context. Thread: {0} AppDomain: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), AppDomain.CurrentDomain.FriendlyName));
                    return new MyContext();
                }
            );                
        }
 
        //control how instances are created
        private MyContext()
        {
        }

        //gets the instance associated with the current thread and app domain
        public static MyContext Current
        {
            get { return _Instance.Value; }
        }

        public int Value1 { get; set; } 

        public override string ToString()
        {
            //explicitly call ToString() so value types aren't boxed
            return string.Format("Value1: {0}", Value1.ToString());
        }
 
    }
}
