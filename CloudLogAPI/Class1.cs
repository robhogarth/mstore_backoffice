using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CloudLogAPI
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
}