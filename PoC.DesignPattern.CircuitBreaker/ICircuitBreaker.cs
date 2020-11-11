using System;
using System.Diagnostics;
using System.Timers;

namespace PoC.DesignPattern.CircuitBreaker
{

    public enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }

    public interface ICircuitBreaker
    {
        CircuitBreakerState State { get; }
        void Reset();
        void Execute(Action action);
        bool IsClosed { get; }
        bool IsOpen { get; }
    }

    public class CircuitBreaker : ICircuitBreaker
    {
        private Timer Timer { get; set; }
        private object monitor = new object();
        private Action Action { get; set; }

        public event EventHandler StateChanged;

        public int Timeout { get; private set; }
        public int Threshold { get; private set; }
        public CircuitBreakerState State { get; private set; }

        public bool IsClosed => State == CircuitBreakerState.Closed;
        public bool IsOpen => State == CircuitBreakerState.Open;

        public int FailureCount { get; private set; }

        public CircuitBreaker(int threshold = 5, int timeout = 6000)
        {
            if (threshold <= 0)
                throw new ArgumentOutOfRangeException($"{threshold} should be greater than zero");

            if (timeout <= 0)
                throw new ArgumentOutOfRangeException($"{timeout} should be greater than zero");

            Threshold = threshold;
            Timeout = timeout;
            State = CircuitBreakerState.Closed;

            Timer = new Timer(timeout);
            Timer.Enabled = false;
            Timer.Elapsed += Timer_Elapsed;
        }

  

        public void Execute(Action action)
        {
            if (State == CircuitBreakerState.Open)
                throw new OpenCircuitException("Circuit breaker is currently open");

            lock(monitor)
            {
                try
                {
                    Action = action;
                    Action();
                }
                catch (Exception ex)
                {
                    if (State == CircuitBreakerState.HalfOpen)
                    {
                        OpenCircuit();
                    }
                    else if (FailureCount <= Threshold)
                    {
                        FailureCount++;

                        if (Timer.Enabled == false)
                            Timer.Enabled = true;
                    }
                    else if (FailureCount >= Threshold)
                        OpenCircuit();

                    throw new CircuitBreakerOperationException("Operation failed", ex);
                }

                if (State == CircuitBreakerState.HalfOpen)
                    Reset();

                if (FailureCount > 0)
                    FailureCount--;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (monitor)
            {
                try
                {
                    Trace.WriteLine($"Retry {FailureCount}");
                    Execute(this.Action);
                    Reset();
                }
                catch
                {
                    if (FailureCount > Threshold)
                    {
                        OpenCircuit();

                        Timer.Elapsed -= Timer_Elapsed;
                        Timer.Enabled = false;
                        Timer.Stop();
                    }
                }
            }
        }

        public void Reset()
        {
            if(State != CircuitBreakerState.Closed)
            {
                Trace.WriteLine($"Circuit Closed");
                ChangeState(CircuitBreakerState.Closed);

                Timer.Stop();
            }
        }

        private void OpenCircuit()
        {
            if(State != CircuitBreakerState.Open)
            {
                Trace.WriteLine($"Open circuit");
                ChangeState(CircuitBreakerState.Open);
            }
        }

        public void ChangeState(CircuitBreakerState state)
        {
            State = state;
            OnCircuitBreakerStateChanged(new EventArgs() { });
        }

        private void OnCircuitBreakerStateChanged(EventArgs e)
        {
            if(StateChanged != null)
            {
                StateChanged(this, e);
            }
        }
    }
}
