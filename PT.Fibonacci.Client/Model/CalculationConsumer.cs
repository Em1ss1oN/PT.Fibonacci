using MassTransit;

namespace PT.Fibonacci.Client.Model
{
    public class CalculationConsumer : IMessageConsumer<ICalculation>
    {
        private readonly ICalculationScheduler _scheduler;

        public CalculationConsumer(ICalculationScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async void Consume(ICalculation message)
        {
            await _scheduler.Schedule(message);
        }
    }
}