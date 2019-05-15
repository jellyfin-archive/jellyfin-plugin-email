using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Security;
using Microsoft.Extensions.Logging;
using MediaBrowser.Plugins.SmtpNotifications.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.SmtpNotifications
{
    public class Notifier : INotificationService
    {
        private readonly ILogger _logger;
        public static Notifier Instance { get; private set; }

        public Notifier(ILogger logger)
        {
            _logger = logger;

            Instance = this;
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && IsValid(options) && options.Enabled;
        }

        private SMTPOptions GetOptions(User user)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.UserId, user.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));
        }

        public string Name
        {
            get { return Plugin.Instance.Name; }
        }


        public Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            return Task.Run(() => TrySendNotification(request, cancellationToken));
        }

        private void TrySendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            try
            {
                SendNotificationCore(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
            }
        }

        private void SendNotificationCore(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            using (var mail = new MailMessage(options.EmailFrom, options.EmailTo)
            {
                Subject = "Emby: " + request.Name,
                Body = string.Format("{0}\n\n{1}", request.Name, request.Description)
            })
            {
                using (var client = new SmtpClient
                {
                    Host = options.Server,
                    Port = options.Port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 20000
                })
                {
                    if (options.SSL) client.EnableSsl = true;

                    _logger.LogInformation("Sending email {to} with subject {subject}", options.EmailTo, mail.Subject);

                    if (options.UseCredentials && !string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                    {
                        client.Credentials = new NetworkCredential(options.Username, options.Password);
                    }
                    else
                    {
                        _logger.LogError("Cannot use credentials for email to {user} because the username or password is missing", options.EmailTo);
                    }

                    try
                    {
                        client.Send(mail);
                        _logger.LogInformation("Completed sending email {to} with subject {subject}", options.EmailTo, mail.Subject);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email");
                    }
                }
            }
        }

        private bool IsValid(SMTPOptions options)
        {
            return !string.IsNullOrEmpty(options.EmailFrom) &&
                   !string.IsNullOrEmpty(options.EmailTo) &&
                   !string.IsNullOrEmpty(options.Server);
        }
    }
}
