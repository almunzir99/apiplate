using apiplate.Domain.Models;
using apiplate.Resources;
using apiplate.Resources.Requests;

namespace apiplate.Domain.Services
{
    public interface IMessagesService : IBaseService<Message,MessageResource,MessageRequestResource>{}




}