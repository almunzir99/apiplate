using apiplate.DataBase;
using apiplate.Models;
using apiplate.Interfaces;
using apiplate.Resources;
using apiplate.Resources.Requests;
using apiplate.Utils.URI;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using apiplate.Repository;

namespace apiplate.Services
{
    public class MessageService : BaseService<Message, MessageResource,MessageRequestResource>, IMessagesService
    {
        public MessageService(IMapper mapper, IRepository<Message> repository, IUriService uriService,IRepository<Admin> _adminsRepository) : base(mapper, uriService, repository,_adminsRepository)
        {
        }
        protected override string GetSearchPropValue(Message obj)
        {
             var type = typeof(Message);
            var usernameProp = type.GetProperties().SingleOrDefault(c => c.Name.ToLower() == "fullname");
            var usernameValue = usernameProp?.GetValue(obj).ToString();
            return usernameValue;
        }
    }
}