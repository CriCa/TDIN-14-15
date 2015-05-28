using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using Utilities;
using Services.ServiceRequest;

namespace BookEditor
{
    public partial class Service : IServiceStore
    {
        public static IServiceStoreCallback callback;

        /*public long connect(string email, string password)
        {
            callback = OperationContext.Current.GetCallbackChannel<IServiceStoreCallback>();

            LocalDatabase.Instance.open();

            Values values = new Values();
            values.add(UserTable.KEY_EMAIL, email);
            values.add(UserTable.KEY_PASSWORD, password);
            long id = -1;

            List<Values> result = UserTable.Instance.get(null, values);
            if (result.Count > 0)
            {
                id = (Int64)result[0].getValue(UserTable.KEY_ID);
                if (!callbacks.ContainsKey(id))
                {
                    callbacks.Add(id, callback);
                }
            }

            return id;
        }*/

        public Books getBooks()
        {
            if(callback == null)
                callback = OperationContext.Current.GetCallbackChannel<IServiceStoreCallback>();

            Books list = new Books();

            List<Values> books = BookTable.Instance.all;

            foreach (Values v in books)
                list.Add(new BookData(
                    (long)v.getValue(BookTable.KEY_ID),
                    (string)v.getValue(BookTable.KEY_TITLE),
                    (long)v.getValue(BookTable.KEY_QUANTITY),
                    (double)v.getValue(BookTable.KEY_PRICE)));

            return list;
        }

        public Response addBook(BookData book)
        {
            Values values = new Values();
            values.add(BookTable.KEY_TITLE, book.title);

            List<Values> books = BookTable.Instance.get(null, values);

            if (books.Count > 0)
                return new Response("error", "That book is already registered!");

            values.clear();

            values.add(BookTable.KEY_TITLE, book.title);
            values.add(BookTable.KEY_QUANTITY, book.quantity);
            values.add(BookTable.KEY_PRICE, book.price);

            BookTable.Instance.insert(values);

            return new Response("success", "Book added!");
        }

        public Response updateBook(BookData book)
        {
            Values values = new Values();

            values.add(BookTable.KEY_ID, book.id);
            values.add(BookTable.KEY_TITLE, book.title);
            values.add(BookTable.KEY_QUANTITY, book.quantity);
            values.add(BookTable.KEY_PRICE, book.price);

            Values values_where = new Values();
            values_where.add(BookTable.KEY_ID, book.id);

            BookTable.Instance.update(values, values_where);

            return new Response("success", "book updated");
        }

        public Response sellBook(BookData book, int quantity)
        {
            Values values = new Values();
            values.add(BookTable.KEY_ID, book.id);

            List<Values> books = BookTable.Instance.get(null, values);

            if (books.Count == 0 || (long) books[0].getValue(BookTable.KEY_QUANTITY) - quantity < 0)
                return new Response("error", "Book doesn't exists or quantity isn't enough");

            values.clear();

            values.add(BookTable.KEY_QUANTITY, (long) books[0].getValue(BookTable.KEY_QUANTITY) - quantity);

            Values values_where = new Values();
            values_where.add(BookTable.KEY_ID, book.id);

            BookTable.Instance.update(values, values_where);

            NetMsmqBinding binding = new NetMsmqBinding();
            binding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            binding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;

            ServiceRequestClient proxy = new ServiceRequestClient(binding, new EndpointAddress("net.msmq://localhost/private/OrderQueue"));
            Request req = new Request();
            req.id = 0;
            req.title = "cenas";
            req.state = 0;
            req.date = "merda";
            req.state_date = "merda2";
            proxy.requestBook(req);

            return new Response("success", "book sold");
        }
    }
}
