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
                values.add(UserTable.KEY_ADDRESS, "Rua Silva Porto n 473");
                values.add(UserTable.KEY_TYPE, 0);
                UserTable.Instance.insert(values);

                values.clear();
                values.add(UserTable.KEY_NAME, "Diogo");
                values.add(UserTable.KEY_EMAIL, "diogo@gmail.com");
                values.add(UserTable.KEY_PASSWORD, "123");
                values.add(UserTable.KEY_ADDRESS, "somewhere xD");
                values.add(UserTable.KEY_TYPE, 0);
                UserTable.Instance.insert(values);
            }

            UserTable.Instance.printTable();

            Console.WriteLine("Press ENTER to terminate the service host");
            Console.ReadLine();

            LocalDatabase.Instance.close();
            host.Close();

            if (MessageQueue.Exists(QUEUE_NAME))
                MessageQueue.Delete(QUEUE_NAME);
        }
    }
}
