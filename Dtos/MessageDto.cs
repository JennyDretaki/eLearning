namespace ELearningApi.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsEdited { get; set; }
    }

    public class CreateMessageDto
    {
        public string ReceiverId { get; set; }
        public string Text { get; set; }
    }
}