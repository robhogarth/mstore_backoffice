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
        Exception,
        Generic
    }

    public class LogMessage
    {
        [JsonProperty("Message")]
        public string Message { get; set; }
        [JsonProperty("lType")]
        public LogType lType { get; set; }
    }

    public class Logger
    {
        public void LogMsg(string message, LogType lType = LogType.Generic)
        {
            LogToAPI(lType, message);
        }

        public void LogException(Exception ex)
        {
            LogToAPI(LogType.Exception, ex.Message);
        }

        public void LogException(string message)
        {
            LogToAPI(LogType.Exception, message);
        }

        private async void LogToAPI(LogType logType, string message)
        {
            string uri = "";

            LogMessage lMessage = new LogMessage()
            {
                Message = message,
                lType = logType
            };
            string jsonData = JsonConvert.SerializeObject(lMessage);
            HttpClient lClient = new HttpClient();
            HttpContent hcontent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            await lClient.PostAsync(uri, hcontent);

            //lClient.PostAsync(Uri,
        }
    }
}
