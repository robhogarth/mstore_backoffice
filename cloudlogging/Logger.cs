using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace cloudlogging
{
    public enum LogType
    {
        ETA,
        Pricing,
        FindUnmatched,
        Generic
    }

    public class Logger
    {
        public async void LogMsg(string message, LogType lType = LogType.Generic)
        {

        }

        public async void LogException(Exception ex)
        {

        }

        public async void LogException(string message)
        {

        }

        private async void LogToAPI(string query)
        {
            string uri = "";
            HttpClient lClient = new HttpClient();
            HttpContent hcontent = new StringContent(query, Encoding.UTF8, "application/json");
            lClient.PostAsync(uri, hcontent);

            //lClient.PostAsync(Uri,
        }


    }
}
