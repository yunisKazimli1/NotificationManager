using NotificationManager.Application.Interfaces;

namespace NotificationManager.Application.Implementations
{
    public class RateLimiter : IRateLimiter
    {
        private readonly Queue<DateTime> _timestamps = new();

        public bool Allow()
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"RateLimiter instance: {GetHashCode()} Count: {_timestamps.Count}");

            while (_timestamps.Count > 0 &&
                   (now - _timestamps.Peek()).TotalMinutes >= 1)
            {
                _timestamps.Dequeue();
            }

            if (_timestamps.Count >= 10)
                return false;

            _timestamps.Enqueue(now);
            return true;
        }
    }
}
