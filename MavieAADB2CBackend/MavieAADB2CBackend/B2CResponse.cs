using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace MavieAADB2CBackend
{
    public class B2CResponse
    {
        public string version { get; set; }
        public int status { get; set; }
        public string userMessage { get; set; }


        public B2CResponse() { }

        public B2CResponse(string message, HttpStatusCode status)
        {
            this.userMessage = message;
            this.status = (int)status;
            this.version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
