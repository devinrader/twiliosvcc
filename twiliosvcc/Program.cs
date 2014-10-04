using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;

namespace twiliosvcc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        public static async Task SendNotifications( [QueueTrigger("twiliosvcc")] List<Notification> notifications) {

            Console.WriteLine("Starting Bulk Notification Delivery");

            var mobileServiceAppUrl = ConfigurationManager.AppSettings["MOBILESERVICEAPPURL"];

            var twilioClient = new TwilioRestClient(ConfigurationManager.AppSettings["ACCOUNTSID"], ConfigurationManager.AppSettings["AUTHTOKEN"]);
            var amsClient = new MobileServiceClient(mobileServiceAppUrl, ConfigurationManager.AppSettings["MOBILESERVICEAPPKEY"]);

            IMobileServiceTable<Notification> notificationsTable = amsClient.GetTable<Notification>();

            foreach (var notification in notifications)
            {
                string notificationCallbackUrl = string.Format("{0}api/notificationCallback?guid={1}", mobileServiceAppUrl, notification.Guid);
                notification.Message = notification.Message + "\r\n\r\nThe message delivered with care by Twilio. Check out twilio.com";
                //have we sent a notification to this phone number before?
                //await notificationsTable.Where(n => n.PhoneNumber == notification.PhoneNumber).ToListAsync();
                
                //if we have set the flag to send the instructions message

                //save this notification    
                await notificationsTable.InsertAsync(notification);
                
                Console.WriteLine("Sending to {0}", notification.PhoneNumber);


                var result = twilioClient.SendMessage(
                    ConfigurationManager.AppSettings["FROM"], 
                    notification.PhoneNumber, 
                    notification.Message,
                    notificationCallbackUrl);

                if (result.RestException != null)
                {
                    Console.WriteLine("Error sending to API: '{0}'", result.RestException.Message);
                    notification.Status = "ApiFail";
                    await notificationsTable.UpdateAsync(notification);
                }
                else
                {
                    Console.WriteLine("Sent to API: '{0}', Status: '{1}'", result.Sid, result.Status);
                    notification.Status = result.Status;
                    notification.MessageSid = result.Sid;
                    await notificationsTable.UpdateAsync(notification);
                }                              
            }
        }
    }
}
