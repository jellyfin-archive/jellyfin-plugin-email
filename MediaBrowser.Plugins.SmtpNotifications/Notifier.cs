using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using MailKit.Net.Smtp;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Plugins.SmtpNotifications.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace MediaBrowser.Plugins.SmtpNotifications
{
    public class Notifier : INotificationService
    {
        private readonly ILogger<Notifier> _logger;

        public Notifier(ILogger<Notifier> logger)
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

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.EmailFrom, options.EmailFrom));
            message.To.Add(new MailboxAddress(options.EmailTo, options.EmailTo));
            message.Subject = "Jellyfin: " + request.Name;
            message.Body = new TextPart("plain")
            {
                Text = $"{request.Name}\n\n{request.Description}"
            };
            
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(options.Server, options.Port, options.SSL, cancellationToken).ConfigureAwait(false);
                if (options.UseCredentials)
                {
                    if (!string.IsNullOrEmpty(options.Username)
                        && !string.IsNullOrEmpty(options.Password))
                    {
                        await client.AuthenticateAsync(options.Username, options.Password, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogError(
                            "Cannot use credentials for email to {User} because the username or password is missing",
                            options.EmailTo);
                    }
                }

                _logger.LogInformation("Sending email {to} with subject {subject}", options.EmailTo, message.Subject);
                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Completed sending email {to} with subject {subject}", options.EmailTo, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
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
