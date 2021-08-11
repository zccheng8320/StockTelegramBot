namespace TelegramBotExtensions.Interfaces
{
    public interface IQueue<T>
    {
        /// <summary>
        /// Thread-Safety 
        /// </summary>
        /// <param name="objects"></param>
        T Dequeue();
        /// <summary>
        /// Thread-Safety 
        /// </summary>
        /// <param name="objects"></param>
        public void Enqueue(T objects);
    }
}