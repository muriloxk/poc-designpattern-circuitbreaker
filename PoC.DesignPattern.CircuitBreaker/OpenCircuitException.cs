using System;
namespace PoC.DesignPattern.CircuitBreaker
{
    public class OpenCircuitException : Exception
    {
        public OpenCircuitException(string message) : base(message) { }
    }
}
