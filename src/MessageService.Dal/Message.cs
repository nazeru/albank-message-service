namespace MessageService.Dal
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public int MessageId { get; set; }

        public Message(string text, int message_id)
        {
            Text = text;
            MessageId = message_id;
            Timestamp = DateTime.UtcNow;
        }

        public Message(int id, string text, int message_id, DateTime timestamp)
        {
            Id = id;
            Text = text;
            MessageId = message_id;
            Timestamp = timestamp;
        }
    }
}
