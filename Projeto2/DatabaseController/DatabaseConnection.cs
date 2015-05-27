using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using Utilities;

namespace DatabaseController
{
    public abstract class DatabaseConnection
    {
        private const string DB_SUFFIX = ".db";
        protected SQLiteConnection connection = null;
        protected List<DatabaseTable> tables = new List<DatabaseTable>();

        public DatabaseConnection()
        {
            bool exists = File.Exists(databaseName() + DB_SUFFIX);

            if (!exists)
                SQLiteConnection.CreateFile(databaseName() + DB_SUFFIX);

            startConnection();
            fillTables();

            if (!exists)
                createTables();
        }

        private void startConnection()
        {
            this.connection = new SQLiteConnection(String.Format("Data Source={0}{1};Version=3;", databaseName(), DB_SUFFIX));
            this.connection.Open();
        }

        public void open() { }

        private void createTables()
        {
            foreach (DatabaseTable table in tables)
            {
                new SQLiteCommand(table.getCreationScript(), connection).ExecuteNonQuery();
            }
        }


        public void insert(string table, Values values)
        {
            StringBuilder builder = new StringBuilder(String.Format("insert into {0} (", table));
            int i = 0, size = values.size;
            List<string> keys = values.keys;

            for (i = 0; i < size; ++i)
            {
                builder.Append(keys[i]);

                if (i < size - 1)
                    builder.Append(", ");
            }

            builder.Append(") values (");

            for (i = 0; i < size; ++i)
            {
                if (values.getType(keys[i]) == typeof(string))
                    builder.Append("'");

                builder.Append(values.getValue(keys[i]));

                if (values.getType(keys[i]) == typeof(string))
                    builder.Append("'");

                if (i < size - 1)
                    builder.Append(", ");
            }

            builder.Append(");");

            Console.WriteLine(builder.ToString());

            new SQLiteCommand(builder.ToString(), connection).ExecuteNonQuery();
        }

        public void update(string table, Values values, Values where_values)
        {
            StringBuilder builder = new StringBuilder(String.Format("update {0} SET ", table));
            int i = 0, size = values.size, where_size = where_values.size;
            List<string> keys = values.keys, where_keys = where_values.keys;

            for (i = 0; i < size; ++i)
            {
                builder.Append(String.Format("{0}=", keys[i]));

                if (values.getType(keys[i]) == typeof(string))
                    builder.Append("'");

                builder.Append(values.getValue(keys[i]));

                if (values.getType(keys[i]) == typeof(string))
                    builder.Append("'");

                if (i < size - 1)
                    builder.Append(", ");
            }

            builder.Append(" WHERE ");

            for (i = 0; i < where_size; ++i)
            {
                builder.Append(String.Format("{0}=", where_keys[i]));

                if (where_values.getType(where_keys[i]) == typeof(string))
                    builder.Append("'");

                builder.Append(where_values.getValue(where_keys[i]));

                if (where_values.getType(where_keys[i]) == typeof(string))
                    builder.Append("'");

                if (i < where_size - 1)
                    builder.Append(" and ");
            }

            builder.Append(";");

            new SQLiteCommand(builder.ToString(), connection).ExecuteNonQuery();
        }

        public List<Values> all(string table)
        {
            List<Values> rows = new List<Values>();

            SQLiteDataReader result = new SQLiteCommand(String.Format("select * from {0};", table), connection).ExecuteReader();
            while (result.Read())
            {
                Values v = new Values();
                
                foreach (string k in result.GetValues().AllKeys)
                    v.add(k, result[k]);
                
                rows.Add(v);
            }

            return rows;
        }

        public List<Values> get(string table, List<string> values, Values where_values)
        {
            List<Values> rows = new List<Values>();
            int size, i;

            StringBuilder builder = new StringBuilder("select ");

            if (values == null || values.Count == 0)
                builder.Append("*");
            else
            {
                size = values.Count;

                for (i = 0; i < size; ++i)
                {
                    builder.Append(values[i]);

                    if (i < size - 1)
                        builder.Append(", ");
                }
            }

            builder.Append(" from " + table);

            if (where_values != null && where_values.size > 0)
            {
                builder.Append(" where ");

                size = where_values.size;
                string k;
                for (i = 0; i < size; ++i)
                {
                    k = where_values.keys[i];
                    builder.Append(k + "=");

                    if (where_values.getType(k) == typeof(string))
                        builder.Append("'");

                    builder.Append(where_values.getValue(k));

                    if (where_values.getType(k) == typeof(string))
                        builder.Append("'");

                    if (i < size - 1)
                        builder.Append(" and ");
                }
            }

            builder.Append(";");

            SQLiteDataReader result = new SQLiteCommand(builder.ToString(), connection).ExecuteReader();
            while (result.Read())
            {
                Values v = new Values();
                
                foreach (string k in result.GetValues().AllKeys)
                    v.add(k, result[k]);
                
                rows.Add(v);
            }

            return rows;
        }

        public void delete(string table, Values where_values)
        {
            int size, i;

            StringBuilder builder = new StringBuilder("delete from " + table);

            if (where_values != null && where_values.size > 0)
            {
                builder.Append(" where ");

                size = where_values.size;
                string k;
                for (i = 0; i < size; ++i)
                {
                    k = where_values.keys[i];
                    builder.Append(k + "=");

                    if (where_values.getType(k) == typeof(string))
                        builder.Append("'");

                    builder.Append(where_values.getValue(k));

                    if (where_values.getType(k) == typeof(string))
                        builder.Append("'");

                    if (i < size - 1)
                        builder.Append(" and ");
                }
            }

            builder.Append(";");
            new SQLiteCommand(builder.ToString(), connection).ExecuteNonQuery();
        }

        public bool close()
        {
            if (this.connection != null)
            {
                this.connection.Close();
                return true;
            }

            return false;
        }

        protected abstract string databaseName();

        protected abstract void fillTables();
    }
}
