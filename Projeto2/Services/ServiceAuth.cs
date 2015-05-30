using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace BookEditor
{
    public partial class Service : IServiceAuth
    {
        public LoginResponse login(LoginData data)
        {
            LocalDatabase.Instance.open();

            Values values = new Values();

            values.add(UserTable.KEY_EMAIL, data.email);
            values.add(UserTable.KEY_PASSWORD, data.password);

            List<Values> users = UserTable.Instance.get(null, values);

            LoginResponse resp = new LoginResponse();

            if (users.Count == 0)
            {
                resp.State = "invalid";
                resp.Message = "invalid email-password combination";
            }
            else
            {
                resp.State = "success";
                resp.Message = "user logged in successfully";
                resp.Id = (long)users[0].getValue(UserTable.KEY_ID);
                resp.Name = (string)users[0].getValue(UserTable.KEY_NAME);
            }

            return resp;
        }

        public Response logout(LogoutData data)
        {
            return new Response("success", "logged out successfully");
        }

        public Response register(RegisterData data)
        {
            LocalDatabase.Instance.open();

            Values values = new Values();
            values.add(UserTable.KEY_EMAIL, data.email);
            List<Values> users = UserTable.Instance.get(null, values);

            if (users.Count == 0)
            {
                values.clear();

                values.add(UserTable.KEY_NAME, data.name);
                values.add(UserTable.KEY_EMAIL, data.email);
                values.add(UserTable.KEY_PASSWORD, data.password);
                UserTable.Instance.insert(values);

                return new Response("success", "new user created");

            }

            return new Response("error", "user already exists");
        }
    }
}
