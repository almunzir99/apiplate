using System.Threading.Tasks;
using apiplate.Domain.Models;
using apiplate.Resources;
using apiplate.Resources.Requests;

namespace apiplate.Domain.Services
{
    public interface IRolesService: IBaseService<Role,RoleResource,RoleRequestResource>{
        Task<Role> GetRoleByTitle(string title);
        
    }
}