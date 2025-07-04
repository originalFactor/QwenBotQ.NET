using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    public partial class OneBot
    {
        public void AddCallback<EventType>(Func<EventType, Task> callback) where EventType : BaseEventModel
        {
            OnEvent += async (eventModel) =>
            {
                if (eventModel is EventType eventType)
                {
                    await callback.Invoke(eventType);
                }
            };
        }
    }
}