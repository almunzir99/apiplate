using System.Linq;
using apiplate.DataBase;
using apiplate.Models;
using apiplate.Resources;
using apiplate.Resources.Requests;
using apiplate.Services.FilesManager;
using apiplate.Utils.SMTP.Services;
using apiplate.Utils.URI;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using apiplate.Interfaces;
using apiplate.Repository;

namespace apiplate.Services
{
    public class AdminService : BaseUserService<Admin, AdminResource, AdminRequestResource>, IAdminService
    {
        public AdminService(IMapper mapper,
        ApiplateDbContext context,
        IRepository<Admin> repository,
        IConfiguration config,
        ISMTPService smtpSerivce,
        IWebHostEnvironment webhostEnvironment,
        IFilesManagerService filesManagerService, 
        IUriService uriService,IRepository<Admin> _adminsRepository) : 
        base(mapper, context, smtpSerivce, config, webhostEnvironment, filesManagerService, uriService,repository,_adminsRepository)
        {
            repository.IncludeableDbSet = repository.IncludeableDbSet.Include(c => c.Activities)
            .Include(c => c.Role).Include(c => c.Role);
        }

        protected override string type => "ADMIN";

    }
}