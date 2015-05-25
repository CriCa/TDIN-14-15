using System;
using DatabaseController;

namespace BookEditor
{
    public class OrderTable : DatabaseTable
    {
        public const string KEY_ID = "id";
        public const string KEY_CLIENT_ID = "client_id";
        public const string KEY_BOOK_ID = "book_id";
        public const string KEY_QUANTITY = "quantity";
        public const string KEY_STATE = "state";
        public const string KEY_DATE = "date";
        public const string KEY_STATE_DATE = "state_date";
        public const string KEY_PRICE = "total_price";

        public const int WAITING = 0;
        public const int DISPATCHED = 1;
        public const int TO_DISPATCH = 2;
        public const int SOLD = 3;

        private static OrderTable instance = new OrderTable();

        public static OrderTable Instance { get { return instance; } }

        private OrderTable() { }

        override protected string getTableName() { return "orders"; }

        override protected string getCreationScript()
        { // TODO correct this
            return "";//String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} TEXT, {3} TEXT NOT NULL, {4} INTEGER NOT NULL DEFAULT 0, {5} TEXT NOT NULL, {6} INTEGER NOT NULL DEFAULT 0, {7} TEXT);", getTableName(), KEY_ID, KEY_NAME, KEY_EMAIL, KEY_TYPE, KEY_PASSWORD, KEY_STATE, KEY_TOKEN);
        }
    }
}
