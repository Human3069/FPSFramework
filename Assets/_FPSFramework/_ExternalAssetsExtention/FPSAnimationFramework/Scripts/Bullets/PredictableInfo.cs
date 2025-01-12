
namespace FPS_Framework
{
    public struct PredictableInfo
    {
        public PredictableInfo(float speed, float drag)
        {
            Speed = speed;
            Drag = drag;
        }

        public float Speed;
        public float Drag;
    }
}