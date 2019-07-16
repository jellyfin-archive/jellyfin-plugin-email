using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Plugins.SmtpNotifications.Configuration;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.SmtpNotifications
{
    public class Notifier : INotificationService
    {
        private readonly ILogger _logger;

        public Notifier(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && IsValid(options) && options.Enabled;
        }

        private SMTPOptions GetOptions(User user)
            => Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.UserId, user.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));

        public string Name => Plugin.Instance.Name;

        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            using (var mail = new MailMessage(options.EmailFrom, options.EmailTo)
            {
                Subject = "Jellyfin: " + request.Name,
                Body = string.Format("{0}\n\n{1}", request.Name, request.Description)
            })
            using (var client = new SmtpClient
            {
                Host = options.Server,
                Port = options.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000
            })
            {
                if (options.SSL)
                {
                    client.EnableSsl = true;
                }

                _logger.LogInformation("Sending email {to} with subject {subject}", options.EmailTo, mail.Subject);

                if (options.UseCredentials
                    && !string.IsNullOrEmpty(options.Username)
                    && !string.IsNullOrEmpty(options.Password))
                {
                    client.Credentials = new NetworkCredential(options.Username, options.Password);
                }
                else
                {
                    _logger.LogError(
                        "Cannot use credentials for email to {User} because the username or password is missing",
                        options.EmailTo);
                }

                try
                {
                    await client.SendMailAsync(mail).ConfigureAwait(false);
                    _logger.LogInformation("Completed sending email {to} with subject {subject}", options.EmailTo, mail.Subject);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email");
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
