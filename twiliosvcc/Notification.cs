using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twiliosvcc
{
    public class Notification
    {
        public Notification()
        {
            this.Status = "New";
            this.MessageSid = "";
            this.ErrorCode = "";
        }

        public string Id { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "messagesid")]
        public string MessageSid { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "errorcode")]
        public string ErrorCode { get; set; }        
    }
}
