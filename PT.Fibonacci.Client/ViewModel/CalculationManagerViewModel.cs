using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PT.Fibonacci.Client.Annotations;
using PT.Fibonacci.Client.Model;

namespace PT.Fibonacci.Client.ViewModel
{
    public class CalculationManagerViewModel : ViewModelBase
    {
        private readonly ICalculationManager _manager;
        
        public CalculationManagerViewModel([NotNull] ICalculationManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            _manager = manager;
            var propertyChanged = manager as INotifyPropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged.PropertyChanged += ManagerOnPropertyChanged;
            }

            Calculations = new ObservableCollection<CalculationViewModel>();
            InitializeCalculationCollection();
            var calculations = _manager.Calculations as INotifyCollectionChanged;
            if (calculations != null)
            {
                calculations.CollectionChanged += CalculationCollectionChange;
            }

            StartStopCommand = new RelayCommand(async (o) => await StartStopCalculations(),
                (o) => CanExecuteStartStop());

            Count = 0;
        }

        public ICommand StartStopCommand { get; }

        public int Count { get; set; }

        public CalculatorStatus Status => _manager.Status;

        public ObservableCollection<CalculationViewModel> Calculations { get; }

        private void ManagerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            CommandManager.InvalidateRequerySuggested();
            if (propertyChangedEventArgs.PropertyName == nameof(Status))
            {
                OnPropertyChanged(nameof(Status));
            }
        }

        private void CalculationCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    InitializeCalculationCollection();
                    break;
                case NotifyCollectionChangedAction.Add:
                    var index = e.NewStartingIndex;
                    foreach (var calculation in e.NewItems.Cast<Calculation>())
                    {
                        Calculations.Insert(index, new CalculationViewModel(calculation));
                        index++;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var calculation in e.OldItems.Cast<Calculation>())
                    {
                        var item = Calculations.FirstOrDefault(i => i.Calculation == calculation);
                        if (item != null)
                        {
                            Calculations.Remove(item);
                            item.Dispose();
                        }
                    }
                    break;
            }
        }
        
        private void InitializeCalculationCollection()
        {
            foreach (var viewModel in Calculations)
            {
                viewModel.Dispose();
            }

            Calculations.Clear();

            foreach (var viewModel in _manager.Calculations.Select(c => new CalculationViewModel(c)))
            {
                Calculations.Add(viewModel);
            }
        }

        private bool CanExecuteStartStop()
        {
            return Status == CalculatorStatus.NotStarted || Status == CalculatorStatus.Started;
        }

        private async Task StartStopCalculations()
        {
            if (Count <= 0)
            {
                MessageBox.Show(@"Count should me more than zero.");
                return;
            }

            if (Status == CalculatorStatus.NotStarted)
            {
                await _manager.Start(Count);
            }
            else if (Status == CalculatorStatus.Started)
            {
                await _manager.Stop();
            }
        }
    }
}
