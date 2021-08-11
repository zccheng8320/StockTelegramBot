namespace Lib
{
    public interface IServiceResult<T>
    {
        T Data { get; }
        ServiceState ServiceState { get; }
    }

    public enum ServiceState
    {
        Success = 0,
        Fail = 1
    }
}