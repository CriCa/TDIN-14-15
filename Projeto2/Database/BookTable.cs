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
        { // TODO correct this
            return "";// String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} TEXT, {3} TEXT NOT NULL, {4} INTEGER NOT NULL DEFAULT 0, {5} TEXT NOT NULL, {6} INTEGER NOT NULL DEFAULT 0, {7} TEXT);", getTableName(), KEY_ID, KEY_NAME, KEY_EMAIL, KEY_TYPE, KEY_PASSWORD, KEY_STATE, KEY_TOKEN);
        }
    }
}
