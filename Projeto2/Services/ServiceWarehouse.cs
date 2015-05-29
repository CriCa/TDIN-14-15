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

            // update
            values.add(OrderTable.KEY_STATE, OrderTable.TO_DISPATCH);
            values.add(OrderTable.KEY_STATE_DATE, Functions.getStringFromDate(DateTime.Now.AddDays(2.0)));

            where_values.add(OrderTable.KEY_ID, request.order_id);

            OrderTable.Instance.update(values, where_values);

            callback.UpdateOrders();

            return new Response("success", "book successfully shipped");
        }
    }
}
