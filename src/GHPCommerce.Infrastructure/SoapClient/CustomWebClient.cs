using System;
using System.Net;

namespace SoapClient
{
    public class CustomWebClient:WebClient
    {
        private int timeout = -1;
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request.GetType() == typeof(HttpWebRequest))
            {   
                ((HttpWebRequest)request).Timeout = timeout;
            }

            return request;
        }  

    }
}
