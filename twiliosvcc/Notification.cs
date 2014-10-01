using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twiliosvcc
{
    public class Notification
    {
        public string Message { get; set; }
        public string PhoneNumber { get; set; }
        public string Guid { get; set; }
        public string Status { get; set; }
    }
}
