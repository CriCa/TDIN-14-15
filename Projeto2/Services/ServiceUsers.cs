using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace BookEditor
{
    public partial class Service : IServiceUsers
    {
        public Orders getOrdersFrom(string id)
        {
            Orders list = new Orders();
            Values where_values = new Values();

            where_values.add(OrderTable.KEY_CLIENT_ID, id);

            List<Values> result = OrderTable.Instance.get(null, where_values);

            foreach(Values v in result) 
            {
                List<string> keys = new List<string>();
                keys.Add(BookTable.KEY_TITLE);
                where_values.clear();
                where_values.add(BookTable.KEY_ID, v.getValue(OrderTable.KEY_BOOK_ID));

                List<Values> res = BookTable.Instance.get(keys, where_values);

                list.Add(new OrderData(
                    (long)v.getValue(OrderTable.KEY_ID),
                    (long)v.getValue(OrderTable.KEY_BOOK_ID),
                    (string)res[0].getValue(BookTable.KEY_TITLE),
                    (long)v.getValue(OrderTable.KEY_CLIENT_ID),
                    (string)"",
                    (long)v.getValue(OrderTable.KEY_QUANTITY),
                    (int)((long)v.getValue(OrderTable.KEY_STATE)),
                    (string)v.getValue(OrderTable.KEY_DATE),
                    (string)v.getValue(OrderTable.KEY_STATE_DATE),
                    (double)v.getValue(OrderTable.KEY_PRICE),
                    (string)v.getValue(OrderTable.KEY_ADDRESS)));
            }

            return list;
        }
    }
}
