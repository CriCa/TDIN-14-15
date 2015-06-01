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
using System.Net.Mail;

namespace BookEditor
{
    public partial class Service : IServiceStore
    {
        public static IServiceStoreCallback callback;

        public Books getBooks()
        {
            if (callback == null)
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

            if (books.Count == 0 || (long)books[0].getValue(BookTable.KEY_QUANTITY) - quantity < 0)
                return new Response("error", "Book doesn't exists or quantity isn't enough");

            values.clear();

            values.add(BookTable.KEY_QUANTITY, (long)books[0].getValue(BookTable.KEY_QUANTITY) - quantity);

            Values values_where = new Values();
            values_where.add(BookTable.KEY_ID, book.id);

            BookTable.Instance.update(values, values_where);

            return new Response("success", "book sold");
        }

        public Response orderBook(BookData book, long quantity, string clientName, string clientEmail, string clientPassword, string address)
        {
            Values values = new Values();
            List<String> keys = new List<string>();
            Values where_values = new Values();
            string cName;

            keys.Add(UserTable.KEY_ID);
            keys.Add(UserTable.KEY_NAME);
            where_values.add(UserTable.KEY_EMAIL, clientEmail);

            List<Values> result = UserTable.Instance.get(keys, where_values);

            if (result.Count == 0)
            {
                if (clientPassword == "" || clientName == "")
                    return new Response("error", "no client with that email and no info provided to create a new one");
                else
                {
                    values.add(UserTable.KEY_NAME, clientName);
                    values.add(UserTable.KEY_EMAIL, clientEmail);
                    values.add(UserTable.KEY_PASSWORD, clientPassword);

                    UserTable.Instance.insert(values);
                }
            }

            result = UserTable.Instance.get(keys, where_values);

            cName = (string)result[0].getValue(UserTable.KEY_NAME);

            values.clear();

            values.add(OrderTable.KEY_BOOK_ID, book.id);
            values.add(OrderTable.KEY_CLIENT_ID, result[0].getValue(UserTable.KEY_ID));
            values.add(OrderTable.KEY_QUANTITY, quantity);
            values.add(OrderTable.KEY_STATE, OrderTable.WAITING);
            values.add(OrderTable.KEY_DATE, Functions.getCurrentDate());
            values.add(OrderTable.KEY_STATE_DATE, "Waiting Expedition");
            values.add(OrderTable.KEY_PRICE, quantity * book.price);
            values.add(OrderTable.KEY_ADDRESS, address);

            OrderTable.Instance.insert(values);

            where_values.clear();
            where_values.add(OrderTable.KEY_BOOK_ID, book.id);
            keys.Clear();
            keys.Add(OrderTable.KEY_QUANTITY);

            result = OrderTable.Instance.get(keys, where_values);
            long bookDemand = 0;

            foreach (Values v in result)
                bookDemand += (long)v.getValue(OrderTable.KEY_QUANTITY);

            keys.Clear();
            where_values.clear();
            keys.Add(BookTable.KEY_QUANTITY);
            where_values.add(BookTable.KEY_ID, book.id);

            List<Values> res = BookTable.Instance.get(keys, where_values);

            bookDemand -= (long)res[0].getValue(BookTable.KEY_QUANTITY);

            where_values.clear();
            where_values.add(RequestTable.KEY_BOOK_ID, book.id);
            where_values.add(RequestTable.KEY_STATE, RequestTable.WAITING);
            keys.Clear();
            keys.Add(RequestTable.KEY_QUANTITY);

            result = RequestTable.Instance.get(keys, where_values);
            long requestQuantity = 0;

            foreach (Values v in result)
                requestQuantity += (long)v.getValue(RequestTable.KEY_QUANTITY);

            where_values.clear();
            where_values.add(RequestTable.KEY_BOOK_ID, book.id);
            where_values.add(RequestTable.KEY_STATE, RequestTable.SHIPPED);
            keys.Clear();
            keys.Add(RequestTable.KEY_QUANTITY);

            result = RequestTable.Instance.get(keys, where_values);

            foreach (Values v in result)
                requestQuantity += (long)v.getValue(RequestTable.KEY_QUANTITY);

            if (requestQuantity < bookDemand)
            {
                values.clear();
                values.add(RequestTable.KEY_ORDER_ID, OrderTable.Instance.all.Count);
                values.add(RequestTable.KEY_BOOK_ID, book.id);
                values.add(RequestTable.KEY_TITLE, book.title);
                values.add(RequestTable.KEY_QUANTITY, 10 * quantity);
                values.add(RequestTable.KEY_STATE, RequestTable.WAITING);
                values.add(RequestTable.KEY_DATE, Functions.getCurrentDate());
                values.add(RequestTable.KEY_STATE_DESC, "Waiting Expedition");

                RequestTable.Instance.insert(values);

                NetMsmqBinding binding = new NetMsmqBinding();
                binding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
                binding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;

                ServiceRequestClient proxy = new ServiceRequestClient(binding, new EndpointAddress("net.msmq://localhost/private/OrderQueue"));

                Request req = new Request();
                req.order_id = RequestTable.Instance.all.Count;
                req.book_id = book.id;
                req.title = book.title;
                req.quantity = 10 * quantity;
                req.state = OrderTable.WAITING;
                req.date = Functions.getCurrentDate();

                proxy.requestBook(req);
            }

            return new Response("success", "book ordered");
        }

        private void sendMail(string clientEmail, string clientName, string subject, string body)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("bookeditor.tdin1415@gmail.com", "Book Editor");

            msg.To.Add(new MailAddress(clientEmail, clientName));

            msg.Priority = MailPriority.High;
            msg.Subject = subject;

            msg.Body = body;

            msg.Body = getHtmlBody(clientName, msg.Body);

            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;

            client.Credentials = new System.Net.NetworkCredential("bookeditor.tdin1415@gmail.com", "tdin1415");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            client.SendAsync(msg, UserTable.Instance.all.Count);
        }

        private string getHtmlBody(string name, string msg)
        {

            string htmlSource = @"<div style=""font-weight:700""> Dear " + name + @",</div>";
            htmlSource += @"</p>
                      <div align=""left"" class=""article-content"">
                        <multiline label=""Description"">";
            htmlSource += msg;
            htmlSource += @"<br/>
                                        Your sincerely,
                                        Book Editor Adminitrators";

            string ret = PreMailer.Net.PreMailer.MoveCssInline(htmlSource).Html;

            return ret;
        }

        public Orders getOrders()
        {
            Orders list = new Orders();

            List<Values> orders = OrderTable.Instance.all;

            foreach (Values order in orders)
            {
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
                    (double)order.getValue(OrderTable.KEY_PRICE),
                    (string)order.getValue(OrderTable.KEY_ADDRESS)));
            }

            return list;
        }

        public Requests getRequests()
        {
            Requests list = new Requests();

            List<Values> requests = RequestTable.Instance.all;

            foreach (Values request in requests)
            {
                list.Add(new RequestData(
                    (long)request.getValue(RequestTable.KEY_ID),
                    (long)request.getValue(RequestTable.KEY_ORDER_ID),
                    (long)request.getValue(RequestTable.KEY_BOOK_ID),
                    (string)request.getValue(RequestTable.KEY_TITLE),
                    (long)request.getValue(RequestTable.KEY_QUANTITY),
                    (int)((long)request.getValue(RequestTable.KEY_STATE)),
                    (string)request.getValue(RequestTable.KEY_DATE),
                    (string)request.getValue(RequestTable.KEY_STATE_DESC)));
            }

            return list;
        }

        public Response ReceivedRequest(RequestData request)
        {
            Values values = new Values();
            Values where_values = new Values();
            List<string> keys = new List<string>();

            keys.Add(BookTable.KEY_QUANTITY);
            where_values.add(BookTable.KEY_ID, request.book_id);

            List<Values> result = BookTable.Instance.get(keys, where_values);
            long count = (long)result[0].getValue(BookTable.KEY_QUANTITY) + request.quantity;

            values.add(BookTable.KEY_QUANTITY, ((long)result[0].getValue(BookTable.KEY_QUANTITY) + request.quantity));

            BookTable.Instance.update(values, where_values);

            keys.Clear();
            values.clear();
            where_values.clear();
            keys.Add(OrderTable.KEY_ID);
            keys.Add(OrderTable.KEY_BOOK_ID);
            keys.Add(OrderTable.KEY_QUANTITY);
            keys.Add(OrderTable.KEY_PRICE);
            keys.Add(OrderTable.KEY_CLIENT_ID);
            keys.Add(OrderTable.KEY_ADDRESS);
            where_values.add(OrderTable.KEY_STATE, OrderTable.TO_DISPATCH);
            where_values.add(OrderTable.KEY_BOOK_ID, request.book_id);

            result = OrderTable.Instance.get(keys, where_values);

            foreach (Values v in result)
            {
                values.clear();
                where_values.clear();
                keys.Clear();

                keys.Add(BookTable.KEY_QUANTITY);
                where_values.add(BookTable.KEY_ID, v.getValue(OrderTable.KEY_BOOK_ID));

                List<Values> res = BookTable.Instance.get(keys, where_values);

                if (count >= (long)v.getValue(OrderTable.KEY_QUANTITY))
                {
                    values.clear();
                    where_values.clear();

                    values.add(OrderTable.KEY_STATE, OrderTable.DISPATCHED);
                    values.add(OrderTable.KEY_STATE_DATE, "Dispatched at " + Functions.getCurrentDate());

                    where_values.add(OrderTable.KEY_ID, v.getValue(OrderTable.KEY_ID));

                    OrderTable.Instance.update(values, where_values);

                    values.clear();
                    where_values.clear();

                    values.add(BookTable.KEY_QUANTITY, (long)res[0].getValue(BookTable.KEY_QUANTITY) - (long)v.getValue(OrderTable.KEY_QUANTITY));
                    count -= (long)v.getValue(OrderTable.KEY_QUANTITY);

                    where_values.add(BookTable.KEY_ID, v.getValue(OrderTable.KEY_BOOK_ID));

                    BookTable.Instance.update(values, where_values);

                    keys.Clear();
                    where_values.clear();

                    keys.Add(UserTable.KEY_EMAIL);
                    keys.Add(UserTable.KEY_NAME);
                    where_values.add(UserTable.KEY_ID, (long)v.getValue(OrderTable.KEY_CLIENT_ID));

                    List<Values> user = UserTable.Instance.get(keys, where_values);

                    keys.Clear();
                    where_values.clear();

                    keys.Add(BookTable.KEY_TITLE);
                    where_values.add(BookTable.KEY_ID, (long)v.getValue(OrderTable.KEY_BOOK_ID));

                    List<Values> book = BookTable.Instance.get(keys, where_values);

                    string mail = "Your order has been dispatched to your address.<br/><br/><span style=\"font-weight:bold\">Order details:</span><br/>Book Title: "
                + (string)book[0].getValue(BookTable.KEY_TITLE) + "<br/>Quantity: " + (long)v.getValue(OrderTable.KEY_QUANTITY) + "<br/>Price: $" + (double)v.getValue(OrderTable.KEY_PRICE) + "<br/>State: "
                + "Dispatched at " + Functions.getCurrentDate() + "<br/>Address:<br/>" + (string)v.getValue(OrderTable.KEY_ADDRESS);


                    sendMail((string)user[0].getValue(UserTable.KEY_EMAIL), (string)user[0].getValue(UserTable.KEY_NAME), "[Book Editor] Order Dispatched", mail);
                }
            }

            values.clear();
            where_values.clear();

            values.add(RequestTable.KEY_STATE, RequestTable.ARRIVED);
            values.add(RequestTable.KEY_STATE_DESC, "Received");

            where_values.add(RequestTable.KEY_ID, request.id);

            RequestTable.Instance.update(values, where_values);

            return new Response("success", "request received");
        }
    }
}