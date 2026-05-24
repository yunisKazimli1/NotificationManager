namespace NotificationManager.Application.Interfaces
{
    public interface IRateLimiter
    {
        public bool Allow();
    }
}
