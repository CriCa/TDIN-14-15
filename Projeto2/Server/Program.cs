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

            Console.WriteLine("Press ENTER to terminate the service host");
            Console.ReadLine();

            LocalDatabase.Instance.close();
            host.Close();

            if (MessageQueue.Exists(QUEUE_NAME))
                MessageQueue.Delete(QUEUE_NAME);
        }
    }
}
