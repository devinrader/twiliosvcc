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
            Status = "New";
        }

        public string Id { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "guid")]
        public string Guid { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
