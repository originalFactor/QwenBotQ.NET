using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Text;

namespace QwenBotQ.NET;

public class UserModel
{
    public required string Id { get; set; }
    public required string Nick { get; set; }
    public int Permission { get; set; } = 0; // 0: normal, 1: admin, 2: owner, 3: global owner
    public string SystemPrompt { get; set; } = "You are a helpful assistant";
    public double Temprature { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;
    public long Coins { get; set; } = 0;
    public DateTime SignExpire { get; set; } = DateTime.MinValue;
    public string? model { get; set; }
    public DateTime ProfileExpire { get; set; } = DateTime.MinValue;
    public double BindPower { get; set; } = 0.0;
    public BindedModel ?Binded { get; set; }
}

public class BindedModel
{
    [BsonElement("id")]
    public required string Ident { get; set; }
    public DateTime Expire { get; set; } = DateTime.MinValue;
}

public class DataBase
{
    string DbPath;
    string DbName;
    MongoClient Client;
    IMongoDatabase Database;
    IMongoCollection<UserModel> UserCollection;

    public DataBase(string dbPath, string dbName)
    {
        DbPath = dbPath;
        DbName = dbName;

        ConventionRegistry.Register(
            "SnakeCaseConventions",
            new ConventionPack
            {
                new SnakeCaseElementNameConvention()
            },
            t => true
        );

        Client = new MongoClient(DbPath);
        Database = Client.GetDatabase(DbName);
        UserCollection = Database.GetCollection<UserModel>("User");
    }

    public async Task<UserModel?> GetUserAsync(string userId)
    {
         return await UserCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }
}

public class SnakeCaseElementNameConvention : IMemberMapConvention
{
    public string Name => "SnakeCaseElementNameConvention";

    public void Apply(BsonMemberMap memberMap)
    {
        var name = memberMap.MemberName;
        var snakeCaseName = ConvertToSnakeCase(name);
        memberMap.SetElementName(snakeCaseName);
    }

    private string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder();
        sb.Append(char.ToLower(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                sb.Append('_');
                sb.Append(char.ToLower(input[i]));
            }
            else
            {
                sb.Append(input[i]);
            }
        }

        return sb.ToString();
    }
}