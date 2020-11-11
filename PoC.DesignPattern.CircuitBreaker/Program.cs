using System;
using System.Diagnostics;
using System.Threading;

namespace PoC.DesignPattern.CircuitBreaker
{
    class Program
    {
        

        static void Main(string[] args)
        {
            var cb = new CircuitBreaker(5, 3000);
            cb.StateChanged += Cb_StateChanged;

            try
            {
                cb.Execute(() =>
                {
                    throw new Exception();
                });
            }
       
            catch (CircuitBreakerOperationException ex)
            {
                Trace.Write(ex);
            }
            catch (OpenCircuitException ex)
            {
                Console.Write($"Open: {cb.IsOpen}");
            }

            Console.Write($"Closed: {cb.IsClosed}");
            Console.Read();
        }

        private static void Cb_StateChanged(object sender, EventArgs e)
        {
            var cb = sender as CircuitBreaker;

            if (cb.IsOpen)
            {
                Thread.Sleep(6000);
                cb.ChangeState(CircuitBreakerState.HalfOpen);
            }
        }
    }
}
