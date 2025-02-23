using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace MessageService.Dal
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Message>> GetAllMessagesAsync()
        {
            var messages = new List<Message>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using var cmd = new NpgsqlCommand("SELECT id, text, message_id, timestamp FROM Messages", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows) return messages;

                while (await reader.ReadAsync())
                {
                    messages.Add(new Message(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetDateTime(3)
                    ));
                }
            }
            return messages;
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, text, message_id, timestamp FROM Messages WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Message(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetDateTime(3)
                );
            }

            return null;
        }

        public async Task AddMessageAsync(Message message)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("INSERT INTO Messages (text, message_id) VALUES (@text, @message_id) RETURNING id, timestamp", conn);
            cmd.Parameters.AddWithValue("@text", message.Text);
            cmd.Parameters.AddWithValue("@message_id", message.MessageId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                message.Id = reader.GetInt32(0);
                message.Timestamp = reader.GetDateTime(1);
            }
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("DELETE FROM Messages WHERE id = @id RETURNING id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<List<Message>> GetMessagesByPeriodAsync(DateTime from, DateTime to)
        {
            var messages = new List<Message>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("SELECT id, text, message_id, timestamp FROM Messages WHERE timestamp BETWEEN @from AND @to ORDER BY timestamp ASC", conn);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new Message(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetDateTime(3)
                ));
            }

            return messages;
        }
    }
}
