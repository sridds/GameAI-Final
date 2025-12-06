namespace Kart.Car
{
    public interface IKartInput
    {
        public float Throttle { get; }
        public float Steering { get; }
        public bool IsDrifting { get; }
        public bool IsBraking { get; }
    }
}