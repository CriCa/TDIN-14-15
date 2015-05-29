using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.ServiceModel;

namespace WarehouseService
{
    [ServiceContract]
    public interface IServiceRequest {
        [OperationContract(IsOneWay = true)]
        void requestBook(Request request);
    }

    [DataContract]
    public class Request {

        [DataMember]
        public long id;

        [DataMember]
        public long order_id;

        [DataMember]
        public long book_id;

        [DataMember]
        public string title;

        [DataMember]
        public long quantity;

        [DataMember]
        public int state;

        [DataMember]
        public string date;

        [DataMember]
        public string state_date;

        public Request(long id, long order_id, long book_id, string title, long quantity, int state, string date, string state_date)
        {
            this.id = id;
            this.order_id = order_id;
            this.book_id = book_id;
            this.title = title;
            this.quantity = quantity;
            this.state = state;
            this.date = date;
            this.state_date = state_date;
        }
    }
}
