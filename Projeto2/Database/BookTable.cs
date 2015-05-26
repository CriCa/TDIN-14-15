using System;
using DatabaseController;

namespace BookEditor
{
    public class BookTable : DatabaseTable
    {
        public const string KEY_ID = "id";
        public const string KEY_TITLE = "title";
        public const string KEY_PRICE = "price";
        public const string KEY_QUANTITY = "quantity";

        private static BookTable instance = new BookTable();

        public static BookTable Instance { get { return instance; } }

        private BookTable() { }

        override protected string getTableName() { return "books"; }

        override protected string getCreationScript()
        {
            return String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} TEXT NOT NULL, {3} INTEGER NOT NULL DEFAULT 0, {4} REAL NOT NULL DEFAULT 0);", getTableName(), KEY_ID, KEY_TITLE, KEY_QUANTITY, KEY_PRICE);
        }
    }
}
