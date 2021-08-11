using System.Collections.Generic;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;

namespace TelegramBotExtensions.LongPolling
{
    public class UpdateQueue : IQueue<Update>
    {
        private readonly Queue<Update> _updateQueue;
        private readonly object _updateQueueLock = new();
        public UpdateQueue()
        {
            _updateQueue = new Queue<Update>();
        }
        public Update Dequeue()
        {
            while (true)
            {
                lock (_updateQueueLock)
                {
                    if (!_updateQueue.TryDequeue(out var update))
                        continue;
                    return update;
                }
            }
        }

        public void Enqueue(Update objects)
        {
            _updateQueue.Enqueue(objects);
        }
    }
}