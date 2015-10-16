using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage;
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
            var config = new JobHostConfiguration();
            config.NameResolver = new QueueNameResolver();
            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds( int.Parse(ConfigurationManager.AppSettings["MAXPOLLINGINTERVAL"]) );
            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }

        public static async Task SendNotifications([QueueTrigger("%targetqueuename%")] List<Notification> notifications)
        {

            Console.WriteLine("Starting Bulk Notification Delivery");

            var mobileServiceAppUrl = ConfigurationManager.AppSettings["MOBILESERVICEAPPURL"];

            var twilioClient = new TwilioRestClient(ConfigurationManager.AppSettings["ACCOUNTSID"], ConfigurationManager.AppSettings["AUTHTOKEN"]);
            var amsClient = new MobileServiceClient(mobileServiceAppUrl, ConfigurationManager.AppSettings["MOBILESERVICEAPPKEY"]);

            IMobileServiceTable<Notification> notificationsTable = amsClient.GetTable<Notification>();

            if (notifications != null) 
            { 

                string notificationCallbackUrl = string.Format("{0}api/notificationCallback", mobileServiceAppUrl);
                string messagefooter = ConfigurationManager.AppSettings["MESSAGEFOOTER"];

                foreach (var notification in notifications)
                {
                    notification.Message = notification.Message + messagefooter;

                    //save this notification    
                    await notificationsTable.InsertAsync(notification);

                    //rudimentary data validation
                     if (string.IsNullOrEmpty(notification.PhoneNumber.Trim()))
                    {
                        notification.Status = "InputFail";
                        await notificationsTable.UpdateAsync(notification);
                    }
                    else
                    {
                        Console.WriteLine("Sending to {0}", notification.PhoneNumber);

                        var result = twilioClient.SendMessage(
                            ConfigurationManager.AppSettings["FROM"],
                            notification.PhoneNumber.Trim(),
                            notification.Message,
                            notificationCallbackUrl);

                        if (result.RestException != null)
                        {
                            Console.WriteLine("Error sending to API: '{0}'", result.RestException.Message);
                            notification.Status = "ApiFail";
                            notification.ErrorCode = result.RestException.Code;
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
    }

    public class QueueNameResolver : INameResolver
    {
        public string Resolve(string name)
        {
            return ConfigurationManager.AppSettings[name].ToString();
        }
    }
}
