using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using Utilities;

namespace DatabaseController
{
    public abstract class DatabaseTable
    {
        private DatabaseConnection connection;

        protected internal abstract string getTableName();
        protected internal abstract string getCreationScript();

        public void connect(DatabaseConnection conn)
        {
            this.connection = conn;
        }

        protected List<string> getColumnsList()
        {
            List<string> list = new List<string>();

            foreach (FieldInfo field in GetType().GetFields().Where(f => f.Name.StartsWith("KEY_")))
            {
                list.Add(field.GetRawConstantValue().ToString());
            }

            return list;
        }

        public void printTable()
        {
            List<Values> res = all;


            Console.WriteLine("----------- " + getTableName() + " ------------");
            foreach (Values val in res)
            {
                foreach (string k in val.keys)
                {
                    Console.WriteLine(String.Format("{0} -> {1}", k, val.getValue(k)));
                }

                Console.WriteLine("");
            }
            Console.WriteLine("------------------------------\n");
        }

        public void insert(Values values)
        {
            connection.insert(getTableName(), values);
        }

        public void update(Values values, Values where_values)
        {
            connection.update(getTableName(), values, where_values);
        }

        public List<Values> all { get { return connection.all(getTableName()); } }

        public bool deleteAll { get { return delete(null); } }

        public List<Values> get(List<String> values, Values where_values)
        {
            return connection.get(getTableName(), values, where_values);
        }

        public bool delete(Values where_values)
        {
            connection.delete(getTableName(), where_values);
            return true;
        }
    }
}
