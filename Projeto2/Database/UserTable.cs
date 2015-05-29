using System;
using DatabaseController;

namespace BookEditor
{
    public class UserTable : DatabaseTable
    {
        public const string KEY_ID = "id";
        public const string KEY_NAME = "name";
        public const string KEY_EMAIL = "email";
        public const string KEY_ADDRESS = "address";
        public const string KEY_PASSWORD = "password";

        private static UserTable instance = new UserTable();

        public static UserTable Instance { get { return instance; } }

        private UserTable() { }

        override protected string getTableName() { return "users"; }

        override protected string getCreationScript()
        {
            return String.Format("create table {0} ({1} INTEGER PRIMARY KEY AUTOINCREMENT, {2} TEXT, {3} TEXT NOT NULL, {4} TEXT, {5} TEXT NOT NULL);", getTableName(), KEY_ID, KEY_NAME, KEY_EMAIL, KEY_ADDRESS, KEY_PASSWORD);
        }
    }
}
