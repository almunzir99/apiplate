using System.Threading.Tasks;
using apiplate.Models;
using apiplate.Interfaces;
using apiplate.Resources;
using apiplate.Resources.Requests;
using apiplate.Utils.URI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using apiplate.Attributes.Permissions;

namespace apiplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : BaseController<Message, MessageResource,MessageRequestResource, IMessagesService>
    {
        private readonly IRolesService _roleService;
        private readonly INotificationService _notificationService;

        public MessagesController(IMessagesService service, IUriService uriSerivce, IRolesService roleService, INotificationService notificationService) : base(service, uriSerivce)
        {
            _roleService = roleService;
            _notificationService = notificationService;
        }

        public override string PermissionTitle => "MessagesPermissions";

        [AllowAnonymous]
        [Permission(false, PermissionTypes.CREATE)]
        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] MessageRequestResource body){
             // Push Notifications
                var notification = new NotificationResource(){
                    Title = "New Message",
                    Message ="There is a new Message submitted, please check messages page",
                    Module = "MESSAGES",
                    Action = "CREATE",
                    Url = "/dashboard/messages"
                };
                await _notificationService.BroadCastNotification(notification,"admin");
            return await base.PostAsync(body);
        }
        
    }

}