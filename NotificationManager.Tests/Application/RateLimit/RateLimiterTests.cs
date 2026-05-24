using FluentAssertions;
using NotificationManager.Application.Implementations;
using Xunit;

namespace NotificationManager.Tests.Application.RateLimit
{
    public class RateLimiterTests
    {
        [Fact]
        public void Should_allow_first_10_requests()
        {
            var limiter = new RateLimiter();
            bool result = true;

            for (int i = 0; i < 10; i++)
            {
                result = limiter.Allow();
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void Should_block_11th_request_within_same_minute()
        {
            var limiter = new RateLimiter();

            for (int i = 0; i < 10; i++)
            {
                limiter.Allow();
            }

            var result = limiter.Allow();

            result.Should().BeFalse();
        }
    }
}