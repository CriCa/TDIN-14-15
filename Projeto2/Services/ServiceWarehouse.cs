using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace BookEditor
{
    public partial class Service : IServiceWarehouse
    {
        public Response ship(RequestData request)
        {
            Values values = new Values();
            Values where_values = new Values();

            values.add(OrderTable.KEY_STATE, OrderTable.TO_DISPATCH);
            values.add(OrderTable.KEY_STATE_DATE, "Should be dispatched on " + Functions.getStringFromDate(DateTime.Now.AddDays(2.0)));

            where_values.add(OrderTable.KEY_BOOK_ID, request.book_id);
            where_values.add(OrderTable.KEY_STATE, OrderTable.WAITING);

            OrderTable.Instance.update(values, where_values);

            where_values.clear();

            where_values.add(OrderTable.KEY_BOOK_ID, request.book_id);
            where_values.add(OrderTable.KEY_STATE, OrderTable.TO_DISPATCH);

            OrderTable.Instance.update(values, where_values);

            values.clear();
            where_values.clear();

            values.add(RequestTable.KEY_STATE, RequestTable.SHIPPED);
            values.add(RequestTable.KEY_STATE_DESC, "In transit");

            where_values.add(RequestTable.KEY_ID, request.order_id);

            RequestTable.Instance.update(values, where_values);

            if(callback != null)
                callback.UpdateOrders();

            return new Response("success", "book successfully shipped");
        }
    }
}
