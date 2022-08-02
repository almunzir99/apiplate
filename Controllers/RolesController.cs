using System.Threading.Tasks;
using apiplate.Interfaces;
using apiplate.Models;
using apiplate.Resources;
using apiplate.Resources.Requests;
using apiplate.Utils.URI;
using Microsoft.AspNetCore.Mvc;

namespace apiplate.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : BaseController<Role, RoleResource,RoleRequestResource, IRolesService>
    {
        private IRolesService _roleService;
        public override string PermissionTitle => "RolesPermissions";

        public RolesController(IRolesService service, IUriService uriService, IRolesService roleService) : base(service, uriService)
        {
            _roleService = roleService;
        }
    }

    
}