using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Services;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.SmtpNotifications.Api
{
    [Route("/Notification/SMTP/Test/{UserID}", "POST", Summary = "Tests SMTP")]
    public class TestNotification : IReturnVoid
    {
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string UserId { get; set; }
    }

    public class ServerApiEndpoints : IService
    {
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;

        public ServerApiEndpoints(IUserManager userManager, ILogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public Task Post(TestNotification request)
        {
            return new Notifier(_logger).SendNotification(new UserNotification
            {
                Date = DateTime.UtcNow,
                Description = "This is a test notification from Jellyfin Server",
                Level = Model.Notifications.NotificationLevel.Normal,
                Name = "Test Notification",
                User = _userManager.GetUserById(Guid.Parse(request.UserId))
            }, CancellationToken.None);
        }
    }
}
