using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace ChannelSimulation.Models
{
    internal class NetworkModel
    {

        // Параметры модели
        public int PacketTransmissionTime { get; private set; } = 10; // Время передачи пакета в тактах
        public int BinomialTrials { get; private set; } = 10;        // Число испытаний (n)
        public double BinomialProbability { get; private set; } = 0.5; // Вероятность успеха (p)

        public event Action<int, Node, Node> OnModelUpdated;

        public int TotalTackts = 0;
        public Node A { get; private set; } = new Node(0);
        public Node B { get; private set; } = new Node(1);

        public HalfDuplexChannel Channel { get; private set; } = new HalfDuplexChannel();

        public NetworkModel()
        {

        }

        private bool _isRunning = false;
        private int _totalPackets = 0;

        public void Configure(int packetTransmissionTime, int binomialTrials, double binomialProbability)
        {
            PacketTransmissionTime = packetTransmissionTime;
            BinomialTrials = binomialTrials;
            BinomialProbability = binomialProbability;
        }
        public void Run(Func<bool> isRunning)
        {
            _isRunning = true;

            while (isRunning())
            {
                // 1 такт моделирования
                OnSimulationStep();

                // Задержка для такта (например, 100 мс)
                TotalTackts++;
                OnModelUpdated?.Invoke(TotalTackts, A, B);
                Thread.Sleep(100);
            }

            _isRunning = false;
        }

        // Шаг моделирования
        private void OnSimulationStep()
        {
            // Генерация новых пакетов для узлов
            GeneratePackets();

            // Проверка занятости канала и передача пакетов
            ProcessQueues();

            UpdateStatistics();
        }

        // Генерация пакетов для узлов
        private void GeneratePackets()
        {
            if (GenerateBinomial(BinomialTrials, BinomialProbability) > 0)
            {
                A.AddPacket(new Packet
                {
                    Id = _totalPackets++,
                    CreationTime = TotalTackts,
                    Priority = A.Priority
                });
            }

            if (GenerateBinomial(BinomialTrials, BinomialProbability) > 0)
            {
                B.AddPacket(new Packet
                {
                    Id = _totalPackets++,
                    CreationTime = TotalTackts,
                    Priority = B.Priority
                });
            }
        }

        // Обработка очередей узлов
        private void ProcessQueues()
        {
            if (!Channel.IsBusy)
            {
                Packet packetToSend = null;

                // Узел A имеет приоритет
                if (A.Queue.Count > 0)
                {
                    packetToSend = A.GetNextPacket();
                    B.WaitTime++;
                }
                else if (B.Queue.Count > 0)
                {
                    packetToSend = B.GetNextPacket();
                }

                if (packetToSend != null)
                {
                    Channel.StartTransmission(PacketTransmissionTime);
                }
            }
            else
            {
                A.WaitTime++;
                B.WaitTime++;
                Channel.EndTransmission();
            }
        }

        // Обновление статистики
        private void UpdateStatistics()
        {
            A.RecordQueueLength(TotalTackts);
            B.RecordQueueLength(TotalTackts);
        }

        // Генерация биномиального распределения
        private int GenerateBinomial(int trials, double probability)
        {
            int successes = 0;
            var random = new Random();
            for (int i = 0; i < trials; i++)
            {
                if (random.NextDouble() < probability)
                    successes++;
                else successes--;
            }
            return successes;
        }


    }
}
