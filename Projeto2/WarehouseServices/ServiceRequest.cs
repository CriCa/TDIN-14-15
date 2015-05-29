using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ServiceModel;
using Utilities;

namespace WarehouseService
{
    public class ServiceRequest : IServiceRequest
    {
        public static event EventHandler RequestEvent;

        [OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)]
        public void requestBook(Request request)
        {
            addRequest(request);

            refreshRequests();
        }

        private void addRequest(Request request)
        {
            Values values = new Values();
            
            values.add(RequestTable.KEY_ORDER_ID, request.order_id);
            values.add(RequestTable.KEY_BOOK_ID, request.book_id);
            values.add(RequestTable.KEY_TITLE, request.title);
            values.add(RequestTable.KEY_QUANTITY, request.quantity);
            values.add(RequestTable.KEY_DATE, request.date);
            values.add(RequestTable.KEY_STATE_DATE, "-");

            RequestTable.Instance.insert(values);
        }

        public void refreshRequests()
        {
            if (RequestEvent != null)
                RequestEvent(null, null);
        }
    }
}
