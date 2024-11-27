namespace _KMH_Framework
{
    public interface IConfig<T> where T : IConfig<T>
    {
        T Parsed();
    }
}