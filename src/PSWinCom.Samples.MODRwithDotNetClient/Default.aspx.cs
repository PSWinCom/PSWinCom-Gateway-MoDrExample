using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ExtensionMethods;
using PSWinCom.Gateway.Client;
using System.Net.Mail;
using System.Configuration;
using log4net;

namespace PSWinCom.Samples.MODRwithDotNetClient
{
    /// <summary>
    /// This is a simple source code example for using the PSWinCom Gateway .Net client
    /// to receive either MO SMS or DR with a simple .aspx page.
    /// For questions, please contact support@pswin.com
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {
        private static ILog log = LogManager.GetLogger("Default");

        protected void Page_Load(object sender, EventArgs e)
        {
            log.InfoFormat("Receiving {0} request!", Request.HttpMethod);
            log.Debug(HttpContext.Current.Request.ToRaw());


            if (Request.HttpMethod == "POST")
            {
                var smsClient = new SMSClient() 
                { 
                    Username = ConfigurationManager.AppSettings["GatewayUsername"], 
                    Password = ConfigurationManager.AppSettings["GatewayPassword"] 
                };

                HandleModrRequestsFromPSWinComGateway(smsClient);
            }
            else
            {
                log.Info("Handling GET as 405 to avoid exceptions from users browsing to the webapp.");
                Response.StatusCode = 405;
                Response.Write("Received GET request");
            }

            log.Info("Finished Request Handling!");      
        }

       
        /// <summary>
        /// In this method, you can se the way we use the IO streams on the request object for letting the
        /// PSWinCom SMS client retrieve any MO or DR objects from the request.
        /// Then, we send a reply SMS and forward info as email.
        /// Normally, we would most often also store this info in some database, but that is beyond the scope
        /// of this sample.
        /// </summary>
        /// <param name="client"></param>
        private void HandleModrRequestsFromPSWinComGateway(SMSClient client)
        {
            client.HandleIncomingMessages(Request.InputStream, Response.OutputStream);

            foreach (Message incomingmessage in client.ReceivedMessages.Values)          
                SendReplyMessageAndForwardEmail(client, incomingmessage);
                    
             foreach (DeliveryReport deliveryreport in client.DeliveryReports.Values)             
                 SendEmail("A Delivery Report from PSWinCom Gateway was received", "text");

      
        }

        private static void SendReplyMessageAndForwardEmail(SMSClient client, Message incomingmessage)
        {
            try
            {
                var outgoingmessage = new Message();
                outgoingmessage.ReceiverNumber = incomingmessage.SenderNumber;
                outgoingmessage.Text = "Message was received! You sent: " + incomingmessage.Text;
                outgoingmessage.SenderNumber = "ModrDemo";
                outgoingmessage.RequestReceipt = true; // Set this to true to receive a DR from the PSWinCom Gateway.

                log.InfoFormat("Sending reply SMS back to sender, text: {0}", outgoingmessage.Text);
                client.Messages.Add(0, outgoingmessage);
                client.SendMessages();

                log.DebugFormat("Sms sending completed, status: {0}", client.Messages[0].Status);
            }
            catch (Exception e)
            {
                // Catch any exception when sending SMS will not interfere with handling the MO SMS request.
                // You can emulate this by for example setting up invalid username/password in web.config
                log.Error("Exception when sending SMS", e);
            }

            SendEmail("An incoming message from PSWinCom Gateway was received", String.Format("From: {0}, Text: {1}", incomingmessage.SenderNumber, incomingmessage.Text));
        }


        private static void SendEmail(string subject, string body)
        {
            try
            {
                log.Info("Sending Email, normally a customer would also store this information in a database.");
                log.DebugFormat("Subject: {0}, Text: {1}", subject, body);
                string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string toEmail = ConfigurationManager.AppSettings["ToEmail"];
                MailMessage mail = new MailMessage(fromEmail, toEmail);
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = ConfigurationManager.AppSettings["SmtpHost"];
                smtpClient.Port = 25;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                mail.Subject = subject;
                mail.Body = body;
                smtpClient.Send(mail);
                log.Debug("Email Sent");
            }
            catch (Exception e)
            {
                // Catch the exception so that not any email server related hiccups stops the handling of the request from PSWinCom Gateway.
                log.Error("Exception when trying to send email", e);
            }
        }         
    }
}