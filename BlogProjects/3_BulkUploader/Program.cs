using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace _3_BulkUploader
{
    public class Program
    {
        static void Main(string[] args)
        {
            //------------------------------------------------------------
            //--- This code demonstrates use of the BulkUploader<T> ------           
            //------------------------------------------------------------

            //doesn't have to be a temporary table
            string tableName = "#tmpPeoples";        
            BulkUploader<Person> peopleUploader = new BulkUploader<Person>(tableName);

            //add mappings - column meta (last arg) does not need specified if table does not need created
            peopleUploader.AddMapping(x => x.First, "First", "varchar(50) NOT NULL");
            peopleUploader.AddMapping(x => x.Last, "Last", "varchar(50) NOT NULL");
            peopleUploader.AddMapping(x => x.Age, "Age", "int NULL");

            //establish db connection...
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                //if table does not already exist, create it...
                peopleUploader.CreateTable(connection);

                //simplest usage - upload 1 million records
                peopleUploader.Upload(connection, GetPeople(1000000));

                //'advanced' usage - use custom configured bulkCopy instance
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection);
                bulkCopy.BulkCopyTimeout = 60;
                peopleUploader.Upload(bulkCopy, GetPeople(1000000));
            }
        }
 
        private static IEnumerable<Person> GetPeople(Int32 howMany)
        {
            for (int i = 0; i < howMany; i++)
            {
                yield return new Person() { First = "FirstXXX", Last = "LastXXX", Age = 21 };
            }
        }

        private static string GetConnectionString()
        {
            SqlConnectionStringBuilder cnxBldr = new SqlConnectionStringBuilder();
            cnxBldr.DataSource = @"WOMBAT\SQLEXPRESS";
            cnxBldr.InitialCatalog = "tempdb";
            cnxBldr.IntegratedSecurity = true;
            return cnxBldr.ToString();
        }
    }

    public class Person
    {
        public string First { get; set; }
        public string Last { get; set; }
        public int? Age { get; set; }
    }
 
    public class BulkUploader<T>
    {
        private readonly string _Table;
        private List<Map<T>> _Mappings = new List<Map<T>>();

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="table">Name of table to upload to</param>
        public BulkUploader(string table)
        {
            _Table = table;
        }

        /// <summary>
        /// Maps a .NET expression to sql server columns
        /// </summary>
        /// <param name="map">Function to extract value</param>
        /// <param name="column">Sql Server column name</param>
        /// <param name="columnMeta">Sql Server column definition options. This can be null if the table does not need to be created</param>
        public void AddMapping(Func<T, object> map, string column, string columnMeta)
        {
            _Mappings.Add(
                new Map<T>
                {
                    Mapping = map,
                    Column = column,
                    ColumnMeta = columnMeta
                }
            );
        }

        /// <summary>
        /// Uses defined mappings to create table
        /// </summary> 
        public void CreateTable(SqlConnection connection)
        {
            if (_Mappings.Any(x => x.ColumnMeta == null))
            {
                throw new ArgumentException("CreateTable can not be set to true because not all mappings define column metadata");
            }
            using (SqlCommand sqlCmd = new SqlCommand(GetTableCreationSQL(), connection))
            {
                sqlCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Uploads records to sql server using supplied connection.
        /// </summary> 
        public void Upload(SqlConnection connection, IEnumerable<T> records)
        {
            SqlBulkCopy bulkCopy = new SqlBulkCopy(connection);
            Upload(bulkCopy, records);
        }

        /// <summary>
        /// Uploads records to sql server using custom instance of SqlBulkCopy. 
        /// Use this overload for customizing bulkcopy options.
        /// </summary> 
        public void Upload(SqlBulkCopy bulkCopy, IEnumerable<T> records)
        {
            bulkCopy.DestinationTableName = _Table;
            for (int i = 0; i < _Mappings.Count; i++)
            {
                bulkCopy.ColumnMappings.Add(i, _Mappings[i].Column);
            }
            var reader = new RecordReader<T>(_Mappings, records);
            bulkCopy.WriteToServer(reader); 
        }

        /// <summary>
        /// Uses mappings to generate sql for generating a corresponding table
        /// </summary> 
        private string GetTableCreationSQL()
        {
            StringBuilder strBldr = new StringBuilder();
            strBldr.AppendFormat("CREATE TABLE {0}(", _Table).AppendLine();

            for (int i = 0; i < _Mappings.Count; i++)
            {
                var map = _Mappings[i];
                strBldr.AppendFormat("   {0} {1},", map.Column, map.ColumnMeta);
                if (i + 1 < _Mappings.Count)
                {
                    strBldr.AppendLine();
                }
            }
            strBldr.AppendLine(")");

            return strBldr.ToString();
        }

        /// <summary>
        /// Simple type used for storing map information.
        /// </summary> 
        private class Map<TRecordToMap>
        {
            public Func<TRecordToMap, object> Mapping { get; set; }
            public string Column { get; set; }
            public string ColumnMeta { get; set; }
        }

        /// <summary>
        /// IDataReader wrapper over a collection of records
        /// </summary> 
        private class RecordReader<TRecord> : System.Data.IDataReader
        {
            private Map<TRecord>[] _Mappings;
            private IEnumerator<TRecord> _Records;
            private Int32 _RecordsAffected = 0;

            public RecordReader(IEnumerable<Map<TRecord>> mappings, IEnumerable<TRecord> records)
            {
                _Mappings = mappings.ToArray();
                _Records = records.GetEnumerator();
            }

            private TRecord CurrentRecord
            {
                get { return _Records.Current; }
            }

            /// <summary>
            /// Gets value from current record using map with same index
            /// </summary> 
            public object GetValue(int i)
            {
                object retVal = null;
                if (i < _Mappings.Length)
                {
                    var map = _Mappings[i];
                    retVal = map.Mapping(CurrentRecord);
                }
                return retVal;
            }

            public Int32 FieldCount
            {
                get { return _Mappings.Length; }
            }

            public bool Read()
            {
                bool retVal = _Records.MoveNext();
                if (retVal)
                {
                    _RecordsAffected++;
                }
                return retVal;
            }

            public Int32 RecordsAffected
            {
                get { return _RecordsAffected; }
            }

            public void Close()
            {
                _Records = null;
            }

            public bool IsClosed
            {
                get { return _Records == null; }
            }

            public void Dispose()
            {
                Close();
            }

            #region IDataReader NotImplemented

            public bool NextResult()
            {
                throw new NotImplementedException();
            }

            public int Depth
            {
                get { throw new NotImplementedException(); }
            }

            public System.Data.DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IDataRecord Not Implemented

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public System.Data.IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public string GetName(int i)
            {
                throw new NotImplementedException();
            }

            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                throw new NotImplementedException();
            }

            public int GetValues(object[] values)
            {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public object this[string name]
            {
                get { throw new NotImplementedException(); }
            }

            public object this[int i]
            {
                get { throw new NotImplementedException(); }
            }

            #endregion

        }

    }
}
