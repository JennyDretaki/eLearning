namespace ELearningApi.Dtos
{
    public class UserChatDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int UnreadCount { get; set; }
    }
}