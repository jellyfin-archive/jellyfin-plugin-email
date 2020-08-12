using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.SmtpNotifications.Api
{
    [ApiController]
    [Route("Notification/SMTP")]
    [Produces(MediaTypeNames.Application.Json)]
    public class SmtpNotificationsController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<SmtpNotificationsController> _logger;
        private readonly ILogger<Notifier> _notifierLogger;

        public SmtpNotificationsController(IUserManager userManager, ILogger<SmtpNotificationsController> logger, ILogger<Notifier> notifierLogger)
        {
            _userManager = userManager;
            _logger = logger;
            _notifierLogger = notifierLogger;
        }

        /// <summary>
        /// Send test SMTP notification.
        /// </summary>
        /// <param name="userId">The user id of the Jellyfin user.</param>
        /// <response code="204">Test SMTP notification send successfully.</response>
        /// <returns>A <see cref="NoContentResult"/> indicating success.</returns>
        [HttpPost("Test/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> SendSmtpTestNotification([FromRoute] string userId)
        {
            await new Notifier(_notifierLogger).SendNotification(new UserNotification
            {
                Date = DateTime.UtcNow,
                Description = "This is a test notification from Jellyfin Server",
                Level = Model.Notifications.NotificationLevel.Normal,
                Name = "Test Notification",
                User = _userManager.GetUserById(Guid.Parse(userId))
            }, CancellationToken.None);

            _logger.LogInformation("Test SMTP notification sent successfully.");

            return NoContent();
        }
    }
}
