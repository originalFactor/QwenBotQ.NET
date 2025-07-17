using Newtonsoft.Json;
using JsonSubTypes;

namespace QwenBotQ.SDK.Models.OneBot.Messages;

[JsonConverter(typeof(JsonSubtypes), "type")]
[JsonSubtypes.KnownSubType(typeof(Segment<TextData>), "text")]
[JsonSubtypes.KnownSubType(typeof(Segment<IdOnlyData>), "face")]
[JsonSubtypes.KnownSubType(typeof(Segment<ImageData>), "image")]
[JsonSubtypes.KnownSubType(typeof(Segment<RecordData>), "record")]
[JsonSubtypes.KnownSubType(typeof(Segment<FileData>), "video")]
[JsonSubtypes.KnownSubType(typeof(Segment<AtData>), "at")]
[JsonSubtypes.KnownSubType(typeof(Segment<PokeData>), "poke")]
[JsonSubtypes.KnownSubType(typeof(Segment<AnonymousData>), "anonymous")]
[JsonSubtypes.KnownSubType(typeof(Segment<ShareData>), "share")]
[JsonSubtypes.KnownSubType(typeof(Segment<ContactData>), "contact")]
[JsonSubtypes.KnownSubType(typeof(Segment<LocationData>), "location")]
[JsonSubtypes.KnownSubType(typeof(Segment<MusicData>), "music")]
[JsonSubtypes.KnownSubType(typeof(Segment<IdOnlyData>), "reply")]
[JsonSubtypes.KnownSubType(typeof(Segment<IdOnlyData>), "forward")]
[JsonSubtypes.KnownSubType(typeof(Segment<RawData>), "xml")]
[JsonSubtypes.KnownSubType(typeof(Segment<RawData>), "json")]
[JsonSubtypes.FallBackSubType(typeof(Segment<BaseData>))]
public class Segment
{
    public required string Type { get; set; }
    public BaseData? Data { get; set; }
}
