using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PT.Fibonacci.Client.Annotations;

namespace PT.Fibonacci.Client.Model
{
    public class Calculation : INotifyPropertyChanged
    {
        private const string NotInitialized = @"Calculation is not initialized.";
        private const string IsCompleted = @"Calculation is completed";

        private readonly CancellationTokenSource _cancellationSource;
        private readonly ICalculator _calculator;
        private readonly IRestCalculationClient _client;
        private volatile int _id;
        private volatile int _current;
        private int _completed;

        public Calculation(ICalculator calculator, IRestCalculationClient client)
        {
            _calculator = calculator;
            _cancellationSource = new CancellationTokenSource();
            _client = client;
            _id = -1;
            _completed = 0;
            Task.Factory.StartNew(async () => await Initialize());
        }

        public Int32 Id
        {
            get { return _id; }
            private set
            {
                _id = value; 
                OnPropertyChanged();
            }
        }

        public int Current
        {
            get { return _current; }
            private set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        public Boolean Completed => _completed != 0;

        public async Task PostNext(int next)
        {
            if (_completed != 0)
            {
                throw new InvalidOperationException(IsCompleted);
            }

            var current = Interlocked.Exchange(ref _current, next);
            OnPropertyChanged(nameof(Current));
            try
            {
                Current = _calculator.Calculate(current, _current);
            }
            catch (InvalidOperationException)
            {
                await Complete();
                return;
            }

            await SendCalculationRequest(Current);
        }

        public async Task Complete()
        {
            if (Interlocked.CompareExchange(ref _completed, 1, 0) != 0)
            {
                return;
            }

            _client.Complete(Id);

            _cancellationSource.Cancel();

            OnPropertyChanged(nameof(Completed));
        }

        private async Task Initialize()
        {
            _current = 0;
            try
            {
                Id = await _client.CreateNew(_cancellationSource.Token);
            }
            catch (OperationCanceledException e)
            {
            }

            await SendCalculationRequest(0);
        }

        private async Task SendCalculationRequest(int current)
        {
            if (Id < 0)
            {
                throw new InvalidOperationException(NotInitialized);
            }

            try
            {
                if (!await _client.PostNext(Id, current, _cancellationSource.Token))
                {
                    await Complete();
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}