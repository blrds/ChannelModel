using ChannelSimulation.Infrastructure.Commands;
using ChannelSimulation.Models;
using ChannelSimulation.ViewModels.Base;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Threading.Tasks;

namespace ChannelSimulation.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        // Свойства модели
        public int PacketTransmissionTime { get; set; } = 2;
        public int BinomialTrials { get; set; } = 5;
        public double BinomialProbability { get; set; } = 0.4;

        private Task _simulationTask;

        // Данные для отображения
        public string StartStopButtonContent => _isRunning ? "Остановить" : "Запустить";

        public int Tacts { get; set; } = 0;
        public int TotalpacketsA { get; set; } = 0;
        public int TransmitedPacketsA { get; set; } = 0;
        public int WaitTimeA { get; set; } = 0;
        public double AverageQLengthA { get; set; } = 0;
        public int TotalpacketsB { get; set; } = 0;
        public int TransmitedPacketsB { get; set; } = 0;
        public int WaitTimeB { get; set; } = 0;
        public double AverageQLengthB { get; set; } = 0;

        public PlotModel QueuePlot { get; private set; }

        // Команды
        public LambdaCommand ToggleSimulationCommand { get; }
        public LambdaCommand ApplySettingsCommand { get; }

        private readonly NetworkModel _networkModel;
        private bool _isRunning = false;

        public MainWindowViewModel()
        {
            _networkModel = new NetworkModel();
            _networkModel.OnModelUpdated += UpdateDataFromModel;

            ToggleSimulationCommand = new LambdaCommand(_ => ToggleSimulation());
            ApplySettingsCommand = new LambdaCommand(_ => ApplySettings());

            QueuePlot = new PlotModel();
            QueuePlot.Series.Add(new LineSeries());
            QueuePlot.Series.Add(new LineSeries());
            QueuePlot.Series[0].Title = "Очередь А";
            QueuePlot.Series[0].RenderInLegend = true;
            QueuePlot.Series[1].Title = "Очередь B";
            QueuePlot.Series[1].RenderInLegend = true;
            QueuePlot.Series.Add(new LineSeries());
            QueuePlot.Series.Add(new LineSeries());
            QueuePlot.Series[2].Title = "Средняя длина очереди А";
            QueuePlot.Series[2].RenderInLegend = true;
            QueuePlot.Series[3].Title = "Средняя длина очереди B";
            QueuePlot.Series[3].RenderInLegend = true;
            _networkModel.A.QueueLengths = ((LineSeries)QueuePlot.Series[0]).Points;
            _networkModel.B.QueueLengths = ((LineSeries)QueuePlot.Series[1]).Points;
            QueuePlot.Legends.Add(new Legend()
            {
            });
            QueuePlot.Legends.Add(new Legend()
            {
            });
            QueuePlot.Legends.Add(new Legend()
            {
            });
            QueuePlot.Legends.Add(new Legend()
            {
            });
        }

        private void ToggleSimulation()
        {
            if (_isRunning)
            {
                // Останавливаем симуляцию
                _isRunning = false;
                OnPropertyChanged(nameof(StartStopButtonContent));
            }
            else
            {
                // Запускаем симуляцию
                _isRunning = true;
                OnPropertyChanged(nameof(StartStopButtonContent));

                // Запуск второго потока
                _simulationTask = Task.Run(() => _networkModel.Run(() => _isRunning));
            }
            OnPropertyChanged(nameof(QueuePlot));
        }

        private void ApplySettings()
        {
            _networkModel.Configure(PacketTransmissionTime, BinomialTrials, BinomialProbability);
        }

        private void UpdateDataFromModel(int t, Node a, Node b)
        {
            Tacts = t;
            TotalpacketsA = a.Totalpackets;
            TransmitedPacketsA = a.TransmitedPackets;
            WaitTimeA = a.WaitTime;
            AverageQLengthA = a.AverageQLength;
            TotalpacketsB = b.Totalpackets;
            TransmitedPacketsB = b.TransmitedPackets;
            WaitTimeB = b.WaitTime;
            AverageQLengthB = b.AverageQLength;
            OnPropertyChanged(nameof(Tacts));
            OnPropertyChanged(nameof(TotalpacketsA));
            OnPropertyChanged(nameof(TotalpacketsB));
            OnPropertyChanged(nameof(WaitTimeA));
            OnPropertyChanged(nameof(WaitTimeB));
            OnPropertyChanged(nameof(TransmitedPacketsA));
            OnPropertyChanged(nameof(TransmitedPacketsB));
            ((LineSeries)QueuePlot.Series[2]).Points.Add(new DataPoint(Tacts, a.AverageQLength));
            ((LineSeries)QueuePlot.Series[3]).Points.Add(new DataPoint(Tacts, b.AverageQLength));
            QueuePlot.InvalidatePlot(true);
        }
    }
}
