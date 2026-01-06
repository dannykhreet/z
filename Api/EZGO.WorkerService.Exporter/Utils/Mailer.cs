using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace EZGO.WorkerService.Exporter.Utils
{
    public class Mailer
    {
        private string SendFrom { get; set; }
        private string SendFromName { get; set; }
        private string SmtpUsername { get; set; }
        private string SmtpPassword { set; get; }
        private string ConfigSet { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }


        public Mailer()
        {
            SendFrom = Environment.GetEnvironmentVariable("MAILER_DEFAULT_FROM");
            SendFromName = Environment.GetEnvironmentVariable("MAILER_DEFAULT_FROM_NAME");
            SmtpUsername = Environment.GetEnvironmentVariable("MAILER_DEFAULT_SMTP_USERNAME");
            SmtpPassword = Environment.GetEnvironmentVariable("MAILER_DEFAULT_SMTP_PASSWORD");
            ConfigSet = Environment.GetEnvironmentVariable("MAILER_DEFAULT_CONFIGSET");
            Host = Environment.GetEnvironmentVariable("MAILER_DEFAULT_SMTP_HOST");
            Port = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAILER_DEFAULT_SMTP_PORT")) ? 587 : Convert.ToInt32(Environment.GetEnvironmentVariable("MAILER_DEFAULT_SMTP_PORT"));
        }

        public string SendMail(string to,  string subject, List<Attachment> attachments = null,  string body = "")
        {

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT").ToLower() != "production" && !to.Contains("@ezfactory.nl"))
            {
                return string.Format("Email NOT send for environment {0} to email {1} due to incorrect email policy.", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT").ToUpper(), to);
            }

            StringBuilder output = new StringBuilder();

            if (!string.IsNullOrEmpty(SmtpUsername) && !string.IsNullOrEmpty(SmtpPassword) && !string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(Host) && Port > 0)
            {
                

                // Create and build a new MailMessage object
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.From = new MailAddress(this.SendFrom, this.SendFromName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                foreach(Attachment attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }
               

                // Comment or delete the next line if you are not using a configuration set
                //message.Headers.Add("X-SES-CONFIGURATION-SET", this.ConfigSet);

                using (var client = new SmtpClient(this.Host, this.Port))
                {
                    // Pass SMTP credentials
                    client.Credentials =
                        new NetworkCredential(this.SmtpUsername, this.SmtpPassword);

                    // Enable SSL encryption
                    client.EnableSsl = true;

                    // Try to send the message. Show status in console.
                    try
                    {
                        output.AppendLine("Attempting to send email...");
                        client.Send(message);
                        output.AppendLine("Email sent!");

                    }
                    catch (Exception ex)
                    {
                        output.AppendLine("[ERROR] The email was not sent.");
                        output.AppendLine(ex.Message);
                    }
                }

                message.Dispose();
                message = null;
            }
            return output.ToString();

        }

        public string SendMail(string To, string subject, Attachment attachment = null, string body = "")
        {
            if(attachment == null)
            {
                return SendMail(To: To, subject: subject, body: body);
            } else
            {
                return SendMail(to: To, attachments: new List<Attachment>() { attachment }, subject: subject, body: body);
            }
            


        }

    }
}
