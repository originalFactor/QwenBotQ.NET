namespace QwenBotQ.SDK.Models.OneBot.API;

public class ApiResponse
{
    public required string Status { get; set; } = "failed";
    public required int Retcode { get; set; } = 0;
    public required long Echo { get; set; }
    public object? Data { get; set; }
}

public class SingleApiResponse : ApiResponse
{
    public new BaseData? Data
    {
        get => (BaseData?)base.Data;
        set => base.Data = value;
    }
}

public class SingleApiResponse<T> : SingleApiResponse
    where T : BaseData
{
    public new T? Data
    {
        get => (T?)base.Data;
        set => base.Data = value;
    }
}

public class MultipleApiResponse : ApiResponse
{
    public new List<BaseData>? Data
    {
        get => (List<BaseData>?)base.Data;
        set => base.Data = value;
    }
}

public class MultipleApiResponse<T> : MultipleApiResponse
    where T : BaseData
{
    public new List<T>? Data
    {
        get => base.Data?.OfType<T>().ToList();
        set => base.Data = value?.Select(d => (BaseData)d).ToList();
    }
}