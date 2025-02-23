using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageService.Dal
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetAllMessagesAsync();
        Task<Message?> GetMessageByIdAsync(int id);
        Task AddMessageAsync(Message message);
        Task<bool> DeleteMessageAsync(int id);
        Task<List<Message>> GetMessagesByPeriodAsync(DateTime from, DateTime to);
    }
}
