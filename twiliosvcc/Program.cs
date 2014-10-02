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
                    Console.WriteLine(result.RestException.Message);
                    notification.Status = "ApiFail";
                    await notificationsTable.UpdateAsync(notification);
                }
                else
                {
                    //if first message, send the instructions email
                }                              
            }
        }
    }
}
