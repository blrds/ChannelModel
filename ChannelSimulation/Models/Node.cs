using OxyPlot;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ChannelSimulation.Models
{
    public class Node
    {
        public Queue<Packet> Queue { get; } = new Queue<Packet>();
        public int Priority { get; }
        public List<DataPoint> QueueLengths = new List<DataPoint>();
        public int Totalpackets = 0;
        public int TransmitedPackets = 0;
        public int WaitTime = 0;
        public double AverageQLength => QueueLengths.Average(x => x.Y);

        public Node(int priority) => Priority = priority;

        public void AddPacket(Packet packet)
        {
            Queue.Enqueue(packet);
            Totalpackets++;
        }

        public Packet GetNextPacket()
        {
            var a = Queue.Count > 0 ? Queue.Dequeue() : null;
            if (a!=null) TransmitedPackets++;
            return a;
        }

        public void RecordQueueLength(int tact) => QueueLengths.Add(new DataPoint(tact, Queue.Count));

    }
}
