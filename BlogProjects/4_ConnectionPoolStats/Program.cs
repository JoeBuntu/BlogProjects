using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections;

namespace _4_ConnectionPoolStats
{
    class Program
    {
        static void Main(string[] args)
        { 
            //same connection string, these will be under the same pool group.
            SqlConnection connection1 = new SqlConnection(GetConnectionString());
            SqlConnection connection2 = new SqlConnection(GetConnectionString());
            SqlConnection connection3 = new SqlConnection(GetConnectionString());
            connection1.Open();
            connection1.Close();
            
            //connection1 will be reused from the pool
            connection2.Open();
            connection3.Open();

            //different connection string = different pool group
            SqlConnection connection4 = new SqlConnection(GetConnectionString() + "BLAH");
            connection4.Open();

            ConnectionPoolHelper.Print();
        }

        private static string GetConnectionString()
        {
            SqlConnectionStringBuilder cnxBldr = new SqlConnectionStringBuilder();
            cnxBldr.DataSource = @"WOMBAT\SQLEXPRESS";
            cnxBldr.InitialCatalog = "tempdb";
            cnxBldr.IntegratedSecurity = true;
            cnxBldr.ApplicationName = "BLAH";
            return cnxBldr.ToString();
        }
    }
     

    public static class ConnectionPoolHelper
    {
        public static void Print()
        {
            Trace.WriteLine("Connection Pool Stats: " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:ffff"));

            //custom binding flags
            BindingFlags privateStatic = BindingFlags.NonPublic | BindingFlags.Static;
            BindingFlags privateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            //use sqlConnection type to get an instance of SqlConnectionFactory
            Type connectionType = typeof(SqlConnection);
            FieldInfo factoryFieldInfo = connectionType.GetField("_connectionFactory", privateStatic);
            object factoryInstance = factoryFieldInfo.GetValue(null);

            //get pool group collection 
            Type factoryType = factoryInstance.GetType().BaseType;
            FieldInfo connectionPoolGroupFieldInfo = factoryType.GetField("_connectionPoolGroups", privateInstance);
            IDictionary connectionPoolGroupTableInstance = (IDictionary) connectionPoolGroupFieldInfo.GetValue(factoryInstance);

            //itereate over pool groups
            Trace.WriteLine("Groups: " + connectionPoolGroupTableInstance.Count.ToString());
            foreach (DictionaryEntry poolGroupKvp in connectionPoolGroupTableInstance)
            {
                Trace.Indent();
  
                //pool groups are keyed by connection string
                object poolGroupIdentity = poolGroupKvp.Key;      
                Type connectionPoolGroupIdentityType = poolGroupIdentity.GetType();
                PropertyInfo connectionPoolIdentityConnectionStringPropertyInfo = connectionPoolGroupIdentityType.GetProperty("ConnectionString", privateInstance);
                string connectionPoolGroupConnectionString = (string) connectionPoolIdentityConnectionStringPropertyInfo.GetValue(poolGroupIdentity, null);
                Trace.WriteLine("Group Key: " + connectionPoolGroupConnectionString);

                //pool group
                object poolGroupInstance = poolGroupKvp.Value;
                Type poolGroupType = poolGroupInstance.GetType();
                FieldInfo poolGroupPoolCollectionFieldInfo = poolGroupType.GetField("_poolCollection", privateInstance);
                IDictionary connectionPoolTableInstance = (IDictionary)poolGroupPoolCollectionFieldInfo.GetValue(poolGroupInstance);
 
                //iterate over pools in this pool group
                Trace.WriteLine("Pools: " + connectionPoolTableInstance.Count.ToString());
                foreach (DictionaryEntry poolKvp in connectionPoolTableInstance)
                {
                    Trace.Indent();

                    //pools are keyed by windows identity (ssid)
                    object poolIdentity = poolKvp.Key;
                    Type poolIdentityType = poolIdentity.GetType();
                    FieldInfo poolIdentitySSIDFieldInfo = poolIdentityType.GetField("_sidString", privateInstance);
                    string poolIdentitySSID = (string) poolIdentitySSIDFieldInfo.GetValue(poolIdentity);
 
                    //get connections from pool
                    object pool = poolKvp.Value;
                    Type poolType = pool.GetType();
                    FieldInfo poolTypeObjectListFieldInfo = poolType.GetField("_objectList", privateInstance);
                    IList poolObjectList = (IList) poolTypeObjectListFieldInfo.GetValue(pool);
 
                    Trace.WriteLine(string.Format("Connections: {0,2} Key: {1}", poolObjectList.Count, poolIdentitySSID));

                    Trace.Unindent();
                }
                Trace.Unindent();
            } 
        }
    }
}
