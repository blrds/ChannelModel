namespace ChannelSimulation.Models
{
    public class HalfDuplexChannel
    {
        public bool IsBusy { get; private set; }

        public int TactsLeft { get; private set; } = 0;
        public void StartTransmission(int tactsLeft) { IsBusy = true; TactsLeft = tactsLeft; }
        public void EndTransmission()
        {
            TactsLeft = TactsLeft == 0 ? 0 : TactsLeft - 1;
            IsBusy = TactsLeft == 0;
        }


    }
}
