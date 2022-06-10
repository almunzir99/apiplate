using apiplate.Domain.Models;
using apiplate.Domain.Services;
using apiplate.Resources;
using apiplate.Resources.Requests;

namespace Studious.Domain.Services
{
    public interface IAdminService : IBaseUserService<Admin,AdminResource,AdminRequestResource>
    {
        
    }
    
}