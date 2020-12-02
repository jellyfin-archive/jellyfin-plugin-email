using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Plugins.SmtpNotifications.Api
{
    [ApiController]
    [Authorize(Policy = "RequiresElevation")]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("Notification/SMTP")]
    public class EmailNotificationController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<Notifier> _notifierLogger;
        
        /// <summary>
        /// Creates a new instance of the <see cref="EmailNotificationController"/>.
        /// </summary>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="notifierLogger">Instance of the <see cref="ILogger{Notifier}"/> interface.</param>
        public EmailNotificationController(
            IUserManager userManager,
            ILogger<Notifier> notifierLogger)
        {
            _userManager = userManager;
            _notifierLogger = notifierLogger;
        }

        /// <summary>
        /// Tests the configured notification.
        /// </summary>
        /// <param name="userId">User to test notification for.</param>
        /// <response code="204">Notification tested successfully.</response>
        /// <returns>A <see cref="NoContentResult"/></returns>
        [HttpPost("Test/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> TestNotification([FromRoute, Required] Guid userId)
        {
            await new Notifier(_notifierLogger).SendNotification(
                new UserNotification
                {
                    Date = DateTime.UtcNow,
                    Description = "This is a test notification from Jellyfin Server",
                    Level = Model.Notifications.NotificationLevel.Normal,
                    Name = "Test Notification",
                    User = _userManager.GetUserById(userId)
                }, CancellationToken.None);

            return NoContent();
        }
    }
}