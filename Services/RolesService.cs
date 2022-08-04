using System.Linq;
using System.Threading.Tasks;
using apiplate.DataBase;
using apiplate.Interfaces;
using apiplate.Models;
using apiplate.Repository;
using apiplate.Resources;
using apiplate.Resources.Requests;
using apiplate.Utils.URI;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apiplate.Services
{
    public class RolesService : BaseService<Role, RoleResource,RoleRequestResource>, IRolesService
    {
        public RolesService(IMapper mapper, IRepository<Role> repository, IUriService uriService,IRepository<Admin> _adminsRepository) : base(mapper, uriService, repository,_adminsRepository)
        {
            repository.IncludeableDbSet = repository.IncludeableDbSet.Include(c => c.MessagesPermissions)
            .Include(c => c.AdminsPermissions)
            .Include(c => c.RolesPermissions);
            
        }

        public async Task<Role> GetRoleByTitle(string title)
        {
            return await _repository.SingleAsync(c => c.Title.Equals(title));
        }

         


    }
}