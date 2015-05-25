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

}