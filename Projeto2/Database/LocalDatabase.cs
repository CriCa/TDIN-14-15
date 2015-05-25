using System;
using DatabaseController;

namespace BookEditor
{
    public class LocalDatabase : DatabaseConnection
    {
        private static LocalDatabase instance = new LocalDatabase();

        public static LocalDatabase Instance { get { return instance; } }

        private LocalDatabase() : base() { }

        override protected string databaseName() { return "BookEditor"; }

        protected override void fillTables()
        {
            this.tables.Add(UserTable.Instance);
            //this.tables.Add(BookTable.Instance);
            //this.tables.Add(OrderTable.Instance);

            foreach (DatabaseTable table in tables) { table.connect(this); }
        }
    }
}
