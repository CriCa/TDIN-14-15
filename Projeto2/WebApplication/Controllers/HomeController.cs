using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            if (Session["email"] == null)
                return View();

            string response = getData(
                "http://localhost:9001/BookEditorServices/users/" + Session["id"] as string + "/orders");

            return View(JsonConvert.DeserializeObject<IEnumerable<OrderModel>>(response));
        }

        public ActionResult Login() {
            ViewBag.Activation = "false";
            return PartialView("_Login");
        }

        public ActionResult Register() {
            ViewBag.RegisterError = "false";
            return PartialView("_Register");
        }

        public ActionResult Create() {
            CreateModel m = new CreateModel();

            string response = getData(
                "http://localhost:9001/BookEditorServices/books/");

            m.books = JsonConvert.DeserializeObject<List<WebApplication.Models.CreateModel.BookModel>>(response);

            ViewBag.CreationError = "false";

            return PartialView("_Create", m);
        }

        public ActionResult Track() {
            string response = getData(
                "http://localhost:9001/BookEditorServices/users/" + Session["id"] as string + "/orders");

            return PartialView("_Track", JsonConvert.DeserializeObject<IEnumerable<OrderModel>>(response));
        }

        public ActionResult Order(long id) {
            string response = getData(
                "http://localhost:9001/BookEditorServices/orders/" + id);

            return PartialView("_Order", JsonConvert.DeserializeObject<OrderModel>(response));
        }
        
        public ActionResult CreateAction(CreateModel m) {
            if (m.quantity != 0 && m.address != null) {

                m.client_id = (long)Session["id"];

                string response = postData(JsonConvert.SerializeObject(m),
                    "http://localhost:9001/BookEditorServices/orders/create");

                response = getData(
                   "http://localhost:9001/BookEditorServices/users/" + Session["id"] as string + "/orders");

                return PartialView("_Track", JsonConvert.DeserializeObject<IEnumerable<Models.OrderModel>>(response));
            }

            string response2 = getData(
                "http://localhost:9001/BookEditorServices/books/");

            ViewBag.CreationError = "true";

            m.books = JsonConvert.DeserializeObject<List<WebApplication.Models.CreateModel.BookModel>>(response2);
            return PartialView("_Create", m);
        }

        public ActionResult RegisterAction(RegisterModel m) {
            string response = postData(JsonConvert.SerializeObject(m),
                "http://localhost:9001/BookEditorServices/auth/register");

            LoginResponseModel rsp = JsonConvert.DeserializeObject<LoginResponseModel>(response);

            if (rsp.State.Equals("success")) {
                return PartialView("_Login");
            }

            ViewBag.RegisterError = "true";

            return PartialView("_Register");
        }

        public ActionResult LoginAction(LoginModel m) {
            string response = postData(JsonConvert.SerializeObject(m),
                "http://localhost:9001/BookEditorServices/auth/login");

            LoginResponseModel rsp = JsonConvert.DeserializeObject<LoginResponseModel>(response);

            if (rsp.State.Equals("success")) {
                Session["id"] = rsp.Id;
                Session["name"] = rsp.Name;
                Session["email"] = m.email;

                string response2 = getData(
                    "http://localhost:9001/BookEditorServices/users/" + Session["id"] as string + "/orders");


                return PartialView("_Track", JsonConvert.DeserializeObject<IEnumerable<OrderModel>>(response2));
            }

            return PartialView("_Login");
        }

        public ActionResult LogoutAction() {
            LogoutModel m = new LogoutModel();
            m.email = Session["email"] as string;

            string response = postData(JsonConvert.SerializeObject(m),
                "http://localhost:9001/BookEditorServices/auth/logout");

            Session.Clear();

            return PartialView("_Login");
        }

        private string getData(string link) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.Method = "GET";

            WebResponse response = request.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private string postData(string data, string link) {
            byte[] dataB = Encoding.UTF8.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.Method = "POST";
            request.ContentType = "application/json";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(dataB, 0, dataB.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }
}