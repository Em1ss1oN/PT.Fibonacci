using System;
using System.ComponentModel;
using PT.Fibonacci.Client.Annotations;
using PT.Fibonacci.Client.Model;

namespace PT.Fibonacci.Client
{
    public class CalculationViewModel : ViewModelBase, IDisposable
    {
        private readonly Calculation _calculation;
        private string _sequence;

        public CalculationViewModel([NotNull] Calculation calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            _calculation = calculation;
            _calculation.PropertyChanged += CalculationOnPropertyChanged;
            Sequence = calculation.Current.ToString();
        }

        public int Id => _calculation.Id;

        public bool Completed => _calculation.Completed;

        public String Sequence
        {
            get { return _sequence; }
            set
            {
                _sequence = value;
                OnPropertyChanged();
            }
        }

        public Calculation Calculation => _calculation;

        private void CalculationOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Calculation.Current))
            {
                Sequence += $", {_calculation.Current}";
            }
            else 
            {
                OnPropertyChanged(propertyChangedEventArgs.PropertyName);
            }
        }

        public void Dispose()
        {
            _calculation.PropertyChanged -= CalculationOnPropertyChanged;
        }
    }
}