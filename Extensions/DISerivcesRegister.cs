using apiplate.Hubs.Connections;
using apiplate.Interfaces;
using apiplate.Models;
using apiplate.Repository;
using apiplate.Services;
using apiplate.Services.ContentManagement;
using apiplate.Services.FilesManager;
using Microsoft.Extensions.DependencyInjection;

namespace apiplate.Extensions
{
    public static class DISerivcesRegister
    {
        public static void RegisterAllRepositoriesScoped(this IServiceCollection services){
            services.AddScoped<IRepository<Admin>,BaseRepository<Admin>>();
            services.AddScoped<IRepository<Message>,BaseRepository<Message>>();
            services.AddScoped<IRepository<Role>,BaseRepository<Role>>();

        }
         public static void RegisterAllServicesScoped(this IServiceCollection services){
           services.AddScoped<IFilesManagerService, FilesManagerService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<IContentManagementService, ContentManagementService>();
            services.AddScoped<IMessagesService, MessageService>();
            services.AddScoped<IConnectionsManager, ConnectionsManager>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

        }
    }
}