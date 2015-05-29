using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace BookEditor
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string Message { get; set; }

        public Response() { }

        public Response(string stt, string msg)
        {
            this.State = stt;
            this.Message = msg;
        }
    }

    [DataContract]
    public class LoginResponse : Response
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public LoginResponse(string stt, string msg, long id, string name)
            : base(stt, msg)
        {
            this.Id = id;
            this.Name = name;
        }

        public LoginResponse() { }
    }

    [DataContract]
    public class LoginData
    {
        [DataMember]
        public string email;

        [DataMember]
        public string password;
    }

    [DataContract]
    public class LogoutData
    {
        [DataMember]
        public string email;
    }

    [DataContract]
    public class RegisterData : LoginData
    {
        [DataMember]
        public string name;
    }

    [DataContract]
    public class BookData
    {
        [DataMember]
        public long id;

        [DataMember]
        public string title;

        [DataMember]
        public long quantity;

        [DataMember]
        public double price;

        public BookData(long id, string title, long quantity, double price)
        {
            this.id = id;
            this.title = title;
            this.quantity = quantity;
            this.price = price;
        }
    }

    [CollectionDataContract]
    public class Books : List<BookData> { }

    [DataContract]
    public class OrderData
    {
        [DataMember]
        public long id;

        [DataMember]
        public long book_id;

        [DataMember]
        public string book_title;

        [DataMember]
        public long client_id;

        [DataMember]
        public string client_email;

        [DataMember]
        public long quantity;

        [DataMember]
        public int state;

        [DataMember]
        public string date;

        [DataMember]
        public string state_date;

        [DataMember]
        public double price;

        [DataMember]
        public string address;

        public OrderData(long id, long book_id, string book_title, long client_id, string client_email, long quantity, int state, string date, string state_date, double price, string address)
        {
            this.id = id;
            this.book_id = book_id;
            this.book_title = book_title;
            this.client_id = client_id;
            this.client_email = client_email;
            this.quantity = quantity;
            this.state = state;
            this.date = date;
            this.state_date = state_date;
            this.price = price;
            this.address = address;
        }
    }

    [CollectionDataContract]
    public class Orders : List<OrderData> { }

    [DataContract]
    public class RequestData
    {

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

        public RequestData(long id, long order_id, long book_id, string title, long quantity, int state, string date, string state_date)
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

    [CollectionDataContract]
    public class Requests : List<RequestData> { }

}