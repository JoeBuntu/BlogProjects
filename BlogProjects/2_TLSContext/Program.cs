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
            //create instance for running threads in the current domain
            Program p1 = new Program();           

            //create a second instance that will execute in a seperate AppDomain
            AppDomain otherDomain = AppDomain.CreateDomain("Other Domain");
            Program p2 = (Program)otherDomain.CreateInstanceFromAndUnwrap("2_TLSContext.exe", "_2_TLSContext.Program");
 
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
 
            Console.ReadLine();
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
