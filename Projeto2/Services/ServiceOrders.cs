using Services.ServiceRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace BookEditor
{
    public partial class Service : IServiceOrders
    {
        public Response CreateOrderFromWeb(CreateData data)
        {
            Values values = new Values();
            List<String> keys = new List<string>();
            Values where_values = new Values();

            bool requestNeeded = false;

            keys.Add(BookTable.KEY_QUANTITY);
            keys.Add(BookTable.KEY_PRICE);
            keys.Add(BookTable.KEY_TITLE);

            where_values.add(BookTable.KEY_ID, data.book_id);

            List<Values> bookRes = BookTable.Instance.get(keys, where_values);

            if ((long)bookRes[0].getValue(BookTable.KEY_QUANTITY) < data.quantity)
                requestNeeded = true;

            if (requestNeeded)
            {
                values.clear();

                values.add(OrderTable.KEY_BOOK_ID, data.book_id);
                values.add(OrderTable.KEY_CLIENT_ID, data.client_id);
                values.add(OrderTable.KEY_QUANTITY, data.quantity);
                values.add(OrderTable.KEY_STATE, OrderTable.WAITING);
                values.add(OrderTable.KEY_DATE, Functions.getCurrentDate());
                values.add(OrderTable.KEY_STATE_DATE, "Waiting Expedition");
                values.add(OrderTable.KEY_PRICE, data.quantity * (double)bookRes[0].getValue(BookTable.KEY_PRICE));
                values.add(OrderTable.KEY_ADDRESS, data.address);

                OrderTable.Instance.insert(values);

                where_values.clear();
                where_values.add(OrderTable.KEY_BOOK_ID, data.book_id);
                keys.Clear();
                keys.Add(OrderTable.KEY_QUANTITY);

                List<Values> result = OrderTable.Instance.get(keys, where_values);
                long bookDemand = 0;

                foreach (Values v in result)
                    bookDemand += (long)v.getValue(OrderTable.KEY_QUANTITY);

                keys.Clear();
                where_values.clear();
                keys.Add(BookTable.KEY_QUANTITY);
                where_values.add(BookTable.KEY_ID, data.book_id);

                List<Values> res = BookTable.Instance.get(keys, where_values);

                bookDemand -= (long)res[0].getValue(BookTable.KEY_QUANTITY);

                where_values.clear();
                where_values.add(RequestTable.KEY_BOOK_ID, data.book_id);
                where_values.add(RequestTable.KEY_STATE, RequestTable.WAITING);
                keys.Clear();
                keys.Add(RequestTable.KEY_QUANTITY);

                result = RequestTable.Instance.get(keys, where_values);
                long requestQuantity = 0;

                foreach (Values v in result)
                    requestQuantity += (long)v.getValue(RequestTable.KEY_QUANTITY);

                where_values.clear();
                where_values.add(RequestTable.KEY_BOOK_ID, data.book_id);
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
                    values.add(RequestTable.KEY_BOOK_ID, data.book_id);
                    values.add(RequestTable.KEY_TITLE, (string)bookRes[0].getValue(BookTable.KEY_TITLE));
                    values.add(RequestTable.KEY_QUANTITY, 10 * data.quantity);
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
                    req.book_id = data.book_id;
                    req.title = (string)bookRes[0].getValue(BookTable.KEY_TITLE);
                    req.quantity = 10 * data.quantity;
                    req.state = OrderTable.WAITING;
                    req.date = Functions.getCurrentDate();

                    proxy.requestBook(req);
                }
            }
            else
            {
                values.clear();

                values.add(OrderTable.KEY_BOOK_ID, data.book_id);
                values.add(OrderTable.KEY_CLIENT_ID, data.client_id);
                values.add(OrderTable.KEY_QUANTITY, data.quantity);
                values.add(OrderTable.KEY_STATE, OrderTable.DISPATCHED);
                values.add(OrderTable.KEY_DATE, Functions.getCurrentDate());
                values.add(OrderTable.KEY_STATE_DATE, "Dispatched at " + Functions.getStringFromDate(DateTime.Now.AddDays(1.0)));
                values.add(OrderTable.KEY_PRICE, data.quantity * (double)bookRes[0].getValue(BookTable.KEY_PRICE));
                values.add(OrderTable.KEY_ADDRESS, data.address);

                OrderTable.Instance.insert(values);

                values.clear();
                where_values.clear();

                values.add(BookTable.KEY_QUANTITY, (long)bookRes[0].getValue(BookTable.KEY_QUANTITY) - data.quantity);

                where_values.add(BookTable.KEY_ID, data.book_id);

                BookTable.Instance.update(values, where_values);

                keys.Clear();
                where_values.clear();
                keys.Add(UserTable.KEY_EMAIL);
                keys.Add(UserTable.KEY_NAME);
                where_values.add(UserTable.KEY_ID, data.client_id);

                List<Values> user = UserTable.Instance.get(keys, where_values);

                string mail = "Your order will be dispatched to your address.<br/><br/><span style=\"font-weight:bold\">Order details:</span><br/>Book Title: "
                + (string)bookRes[0].getValue(BookTable.KEY_TITLE) + "<br/>Quantity: " + data.quantity + "<br/>Price: $" + data.quantity * (double)bookRes[0].getValue(BookTable.KEY_PRICE) + "<br/>State: "
                + "Dispatched at " + Functions.getStringFromDate(DateTime.Now.AddDays(1.0)) + "<br/>Address:<br/>" + data.address;


                sendMail((string)user[0].getValue(UserTable.KEY_EMAIL), (string)user[0].getValue(UserTable.KEY_NAME), "[Book Editor] Order Dispatched", mail);
            }

            if(callback != null)
                callback.UpdateOrders();

            return new Response("success", "book ordered");
        }

        public OrderData GetOrder(string id)
        {
            Values where_values = new Values();

            where_values.add(OrderTable.KEY_ID, id);

            List<Values> result = OrderTable.Instance.get(null, where_values);

            where_values.clear();
            List<string> keys = new List<string>();

            keys.Add(BookTable.KEY_TITLE);
            where_values.add(BookTable.KEY_ID, result[0].getValue(OrderTable.KEY_BOOK_ID));

            List<Values> book = BookTable.Instance.get(keys, where_values);

            return new OrderData(
                (long)result[0].getValue(OrderTable.KEY_ID),
                (long)result[0].getValue(OrderTable.KEY_BOOK_ID),
                (string)book[0].getValue(BookTable.KEY_TITLE),
                (long)result[0].getValue(OrderTable.KEY_CLIENT_ID),
                "",
                (long)result[0].getValue(OrderTable.KEY_QUANTITY),
                (int)((long)result[0].getValue(OrderTable.KEY_STATE)),
                (string)result[0].getValue(OrderTable.KEY_DATE),
                (string)result[0].getValue(OrderTable.KEY_STATE_DATE),
                (double)result[0].getValue(OrderTable.KEY_PRICE),
                (string)result[0].getValue(OrderTable.KEY_ADDRESS));
        }
    }
}
