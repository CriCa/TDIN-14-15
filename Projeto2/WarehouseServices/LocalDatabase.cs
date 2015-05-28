using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseController;

namespace WarehouseService
{
    public class LocalDatabase : DatabaseConnection
    {
        private static LocalDatabase instance = new LocalDatabase();

        public static LocalDatabase Instance { get { return instance; } }

        private LocalDatabase() : base() { }

        override protected string databaseName() { return "Warehouse"; }

        protected override void fillTables()
        {
            this.tables.Add(RequestTable.Instance);

            foreach (DatabaseTable table in tables) { table.connect(this); }
        }
    }
}
