using System;
namespace PoC.DesignPattern.CircuitBreaker
{
    public class CircuitBreakerOperationException : Exception
    {
        public CircuitBreakerOperationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
