using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PT.Fibonacci.Client.Annotations;

namespace PT.Fibonacci.Client.Model
{
    public interface ICalculationManager
    {
        CalculatorStatus Status { get; set; }
        IEnumerable<Calculation> Calculations { get; }
        Task Start(Int32 count);
        Task Stop();
    }

    public class CalculationManager : ICalculationScheduler, INotifyPropertyChanged, ICalculationManager
    {
        private readonly ObservableCollection<Calculation> _calculations;
        private readonly ReadOnlyObservableCollection<Calculation> _readOnlyCalculations;
        private readonly ICalculator _calculator;
        private readonly IRestCalculationClient _calculationClient;
        private readonly ReaderWriterLockSlim _lock;

        private CancellationTokenSource _cancellationTokenSource;
        private int _calculatorStatus;

        public CalculationManager(ICalculator calculator, IRestCalculationClient calculationClient)
        {
            _calculations = new ObservableCollection<Calculation>();
            _readOnlyCalculations = new ReadOnlyObservableCollection<Calculation>(_calculations);
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _calculator = calculator;
            _calculationClient = calculationClient;
            Status = CalculatorStatus.NotStarted;
        }


        public CalculatorStatus Status
        {
            get { return (CalculatorStatus) _calculatorStatus; }
            set
            {
                Interlocked.Exchange(ref _calculatorStatus, (int) value);
                OnPropertyChanged();
            }
        }

        public IEnumerable<Calculation> Calculations
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _readOnlyCalculations;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public async Task Start(Int32 count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (Interlocked.CompareExchange(ref _calculatorStatus, (int)CalculatorStatus.Starting, (int)CalculatorStatus.NotStarted) != (int)CalculatorStatus.NotStarted)
            {
                throw new InvalidOperationException(@"Calculations already started.");
            }

            OnPropertyChanged(nameof(Status));
            
            _lock.EnterWriteLock();
            try
            {
                _calculations.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            for (int i = 0; i < count; i++)
            {
                _lock.EnterWriteLock();
                try
                {
                    _calculations.Add(new Calculation(_calculator, _calculationClient));
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            Status = CalculatorStatus.Started;
        }

        public async Task Stop()
        {
            if (Interlocked.CompareExchange(ref _calculatorStatus, (int)CalculatorStatus.Stopping, (int)CalculatorStatus.Started) != (int)CalculatorStatus.Started)
            {
                throw new InvalidOperationException(@"Calculations not started.");
            }

            foreach (var calculation in Calculations)
            {
                await calculation.Complete();
            }

            Status = CalculatorStatus.NotStarted;
        }

        public async Task Schedule(ICalculation message)
        {
            if (Status != CalculatorStatus.Started)
            {
                return;
            }

            var calculation = Calculations.FirstOrDefault(c => c.Id == message.Id);
            if (calculation == null)
            {
                return;
            }

            await calculation.PostNext(message.Current);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum CalculatorStatus
    {
        NotStarted,
        Starting,
        Started,
        Stopping
    }

    public interface ICalculationScheduler
    {
        Task Schedule(ICalculation message);
    }
}
