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

            return new Response("success", "book sold");
        }

        public Response orderBook(BookData book, string clientEmail, int quantity)
        {
            Values values = new Values();

            values.add(OrderTable.KEY_BOOK_ID, book.id);
            values.add(OrderTable.KEY_CLIENT_ID, (long) 1);
            values.add(OrderTable.KEY_QUANTITY, book.quantity);
            values.add(OrderTable.KEY_STATE, OrderTable.WAITING);
            values.add(OrderTable.KEY_DATE, Functions.getCurrentDate());
            values.add(OrderTable.KEY_STATE_DATE, Functions.getCurrentDate());
            values.add(OrderTable.KEY_PRICE, quantity * book.price);

            OrderTable.Instance.insert(values);

            // TODO create orders and send mail to user

            NetMsmqBinding binding = new NetMsmqBinding();
            binding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            binding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;

            ServiceRequestClient proxy = new ServiceRequestClient(binding, new EndpointAddress("net.msmq://localhost/private/OrderQueue"));
            
            Request req = new Request();
            req.order_id = OrderTable.Instance.all.Count;
            req.book_id = book.id;
            req.title = book.title;
            req.quantity = 10 * quantity;
            req.state = OrderTable.WAITING;
            req.date = Functions.getCurrentDate();
            
            proxy.requestBook(req);

            return new Response("success", "book ordered");
        }

        public Orders getOrders()
        {
            Orders list = new Orders();

            List<Values> orders = OrderTable.Instance.all;

            foreach (Values order in orders) {
                List<string> values = new List<string>();
                Values where_values = new Values();

                values.Add(BookTable.KEY_TITLE);
                where_values.add(BookTable.KEY_ID, order.getValue(OrderTable.KEY_BOOK_ID));

                List<Values> book = BookTable.Instance.get(values, where_values);

                values.Clear(); where_values.clear();

                values.Add(UserTable.KEY_EMAIL);
                where_values.add(UserTable.KEY_ID, order.getValue(OrderTable.KEY_CLIENT_ID));

                List<Values> user = UserTable.Instance.get(values, where_values);

                list.Add(new OrderData(
                    (long)order.getValue(OrderTable.KEY_ID),
                    (long)order.getValue(OrderTable.KEY_BOOK_ID),
                    (string)book[0].getValue(BookTable.KEY_TITLE),
                    (long)order.getValue(OrderTable.KEY_CLIENT_ID),
                    (string)user[0].getValue(UserTable.KEY_EMAIL),
                    (long)order.getValue(OrderTable.KEY_QUANTITY),
                    (int)((long)order.getValue(OrderTable.KEY_STATE)),
                    (string)order.getValue(OrderTable.KEY_DATE),
                    (string)order.getValue(OrderTable.KEY_STATE_DATE),
                    (double)order.getValue(OrderTable.KEY_PRICE)));
            }

            return list;
        }

        public Response ReceivedRequest(OrderData order)
        {

            return new Response("success", "request received");
        }

        public Response dispatchOrder(OrderData order)
        {


            return new Response("success", "book ordered");
        }
    }
}
