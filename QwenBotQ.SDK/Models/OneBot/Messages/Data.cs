using Newtonsoft.Json;
using JsonSubTypes;

namespace QwenBotQ.SDK.Models.OneBot.Messages;

public class BaseData { }

public class TextData : BaseData
{
    public required string Text { get; set; }
}

public class FileData : BaseData
{
    public required string File { get; set; }
    public string? Url { get; set; }
    public bool? Cache { get; set; }
    public bool? Proxy { get; set; }
    public int? Timeout { get; set; }
}

public class ImageData : FileData
{
    public string? Type { get; set; }
}

public class RecordData : FileData
{
    public required bool magic { get; set; } = false;
}

public class PokeData : BaseData
{
    public required string Type { get; set; }
    public required string Id { get; set; }
    public string? Name { get; set; }
}

public class AtData : BaseData
{
    public required string Qq { get; set; }
}

public class AnonymousData : BaseData
{
    public bool? Ignore { get; set; }
}

public class ShareData : BaseData
{
    public required string Url { get; set; }
    public required string Title { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
}

public class ContactData : BaseData
{
    public required string Id { get; set; }
    public required string Type { get; set; }
}

public class LocationData : BaseData
{
    public required string Lat { get; set; }
    public required string Lon { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}

[JsonConverter(typeof(JsonSubtypes), "type")]
[JsonSubtypes.KnownSubType(typeof(CustomMusicData), "custom")]
[JsonSubtypes.FallBackSubType(typeof(MusicData))]
public class MusicData : BaseData
{
    public string? Type { get; set; }
}

public class CustomMusicData : MusicData
{
    public string? Url { get; set; }
    public string? Audio { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Image { get; set; }
}

public class IdOnlyData : BaseData
{
    public required string Id { get; set; }
}

public class CustomNodeData : BaseData
{
    public required string UserId { get; set; }
    public required string Nickname { get; set; }
    public required List<Segment> Content { get; set; }
}

public class RawData : BaseData
{
    public required string Data { get; set; }
}