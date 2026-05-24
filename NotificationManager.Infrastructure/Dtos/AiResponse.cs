namespace NotificationManager.Infrastructure.Dtos
{
    public class AiResponse
    {
        public required Choice[] Choices { get; set; }

        public class Choice
        {
            public required Message Message { get; set; }
        }

        public class Message
        {
            public required string Content { get; set; }
        }
    }
}
