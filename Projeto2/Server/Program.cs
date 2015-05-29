using System;
using System.Collections.Generic;
using System.Messaging;
using System.ServiceModel;
using System.ServiceModel.Description;
using Utilities;

namespace BookEditor
{
    class Program
    {
        private const string QUEUE_NAME = ".\\private$\\OrderQueue";

        static void Main(string[] args)
        {
            if (!MessageQueue.Exists(QUEUE_NAME))
                MessageQueue.Create(QUEUE_NAME, true);

            ServiceHost host = new ServiceHost(typeof(Service));
            LocalDatabase.Instance.open();
            host.Open();

            Values values = new Values();

            if (UserTable.Instance.all.Count == 0)
            {
                values.clear();
                values.add(UserTable.KEY_NAME, "Cristiano Carvalheiro");
                values.add(UserTable.KEY_EMAIL, "cristiano@gmail.com");
                values.add(UserTable.KEY_PASSWORD, "123");
                values.add(UserTable.KEY_ADDRESS, "Rua Silva Porto n 473"); // TODO ver address na order
                UserTable.Instance.insert(values);

                values.clear();
                values.add(UserTable.KEY_NAME, "Diogo");
                values.add(UserTable.KEY_EMAIL, "diogo@gmail.com");
                values.add(UserTable.KEY_PASSWORD, "123");
                values.add(UserTable.KEY_ADDRESS, "somewhere xD");
                UserTable.Instance.insert(values);
            }

            if (BookTable.Instance.all.Count == 0)
            {
                values.clear();
                values.add(BookTable.KEY_TITLE, "Era uma vez");
                values.add(BookTable.KEY_QUANTITY, "1");
                values.add(BookTable.KEY_PRICE, "25.0");
                BookTable.Instance.insert(values);

                values.clear();
                values.add(BookTable.KEY_TITLE, "Randomness");
                values.add(BookTable.KEY_QUANTITY, "3");
                values.add(BookTable.KEY_PRICE, "22.2");
                BookTable.Instance.insert(values);
            }

            UserTable.Instance.printTable();
            BookTable.Instance.printTable();

            Console.WriteLine("Press ENTER to terminate the service host");
            Console.ReadLine();

            LocalDatabase.Instance.close();
            host.Close();

            if (MessageQueue.Exists(QUEUE_NAME))
                MessageQueue.Delete(QUEUE_NAME);
        }
    }
}
