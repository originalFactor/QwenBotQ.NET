namespace QwenBotQ.SDK.Models.OneBot.Messages;

public class Segment<T> : Segment
    where T : BaseData
{
    public new T? Data
    {
        get => (T?)base.Data;
        set => base.Data = value;
    }
}
