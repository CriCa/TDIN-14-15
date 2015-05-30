using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class OrderModel
    {
        public long id { get; set; }
        public long book_id { get; set; }
        public string book_title { get; set; }
        public long client_id { get; set; }
        public string client_email { get; set; }
        public long quantity { get; set; }
        public int state { get; set; }
        public string date { get; set; }
        public string state_date { get; set; }
        public double price { get; set; }
        public string address { get; set; }

        public OrderModel(long id, long book_id, string book_title, long client_id, string client_email, long quantity, int state, string date, string state_date, double price, string address)
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
}