using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        /*public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }*/

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

        /*public ActionResult Create() {
            CreateModel m = new CreateModel();

            string response = getData(
                "http://localhost:9001/TroubleTicketsServices/categories/");

            m.categories = JsonConvert.DeserializeObject<List<WebApplication.Models.CreateModel.Category>>(response);

            ViewBag.CreationError = "false";

            return PartialView("_Create", m);
        }*/

        public ActionResult Track() {
            string response = getData(
                "http://localhost:9001/TroubleTicketsServices/users/" + Session["id"] as string + "/tickets");

            return PartialView("_Track", JsonConvert.DeserializeObject<IEnumerable<OrderModel>>(response));
        }

        /*public ActionResult Ticket(long id) {
            string response = getData(
                "http://localhost:9001/TroubleTicketsServices/tickets/" + id);

            return PartialView("_Ticket", JsonConvert.DeserializeObject<TicketModel>(response));
        }

        public ActionResult CreateAction(CreateModel m) {
            if (m.title != null && m.description != null) {

                m.user_id = (long)Session["id"];

                string response = postData(JsonConvert.SerializeObject(m),
                    "http://localhost:9001/TroubleTicketsServices/tickets/create");

                response = getData(
                   "http://localhost:9001/TroubleTicketsServices/users/" + Session["id"] as string + "/tickets");

                return PartialView("_Track", JsonConvert.DeserializeObject<IEnumerable<Models.TicketModel>>(response));
            }

            string response2 = getData(
                "http://localhost:9001/TroubleTicketsServices/categories/");

            ViewBag.CreationError = "true";

            m.categories = JsonConvert.DeserializeObject<List<WebApplication.Models.CreateModel.Category>>(response2);
            return PartialView("_Create", m);
        }*/

        public ActionResult RegisterAction(RegisterModel m) {
            string response = postData(JsonConvert.SerializeObject(m),
                "http://localhost:9001/BookEditorServices/auth/register");

            LoginResponseModel rsp = JsonConvert.DeserializeObject<LoginResponseModel>(response);

            if (rsp.State.Equals("success")) {
                ViewBag.Activation = "true";
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

        /*
        public ActionResult LogoutAction() {
            LogoutModel m = new LogoutModel();
            m.email = Session["email"] as string;

            string response = postData(JsonConvert.SerializeObject(m),
                "http://localhost:9001/TroubleTicketsServices/accounts/logout");

            Session.Clear();

            return PartialView("_Login");
        }*/

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