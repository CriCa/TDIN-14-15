using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseController;

namespace WarehouseService
{
    public class RequestTable : DatabaseTable
    {
        public const string KEY_ID = "id";
        public const string KEY_ORDER_ID = "order_id";
        public const string KEY_BOOK_ID = "book_id";
        public const string KEY_TITLE = "title";
        public const string KEY_QUANTITY = "quantity";
        public const string KEY_STATE = "state";
        public const string KEY_DATE = "date";
        public const string KEY_STATE_DATE = "state_date";

        public const int WAITING = 0;
        public const int SHIPPED = 1;

        private static RequestTable instance = new RequestTable();

        public static RequestTable Instance { get { return instance; } }

        private RequestTable() { }

        override protected string getTableName() { return "requests"; }

        override protected string getCreationScript()
        {
            return String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} INTEGER NOT NULL, {3} INTEGER NOT NULL, {4} TEXT NOT NULL, {5} INTEGER NOT NULL DEFAULT 0, {6} INTEGER NOT NULL DEFAULT 0, {7} TEXT, {8} TEXT);", getTableName(), KEY_ID, KEY_ORDER_ID, KEY_BOOK_ID, KEY_TITLE, KEY_QUANTITY, KEY_STATE, KEY_DATE, KEY_STATE_DATE);
        }
    }
}
