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
        public const string KEY_TITLE = "title";
        public const string KEY_QUANTITY = "quantity";
        public const string KEY_STATE = "state";
        public const string KEY_DATE = "date";
        public const string KEY_STATE_DATE = "state_date";

        public const int WAITING = 0;
        public const int DISPATCHED = 1;
        public const int TO_DISPATCH = 2;
        public const int SOLD = 3;

        private static RequestTable instance = new RequestTable();

        public static RequestTable Instance { get { return instance; } }

        private RequestTable() { }

        override protected string getTableName() { return "requests"; }

        override protected string getCreationScript()
        {
            return String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} TEXT NOT NULL, {3} INTEGER NOT NULL DEFAULT 0, {4} INTEGER NOT NULL DEFAULT 0, {5} TEXT, {6} TEXT NOT NULL);", getTableName(), KEY_ID, KEY_TITLE, KEY_QUANTITY, KEY_STATE, KEY_DATE, KEY_STATE_DATE);
        }
    }
}
