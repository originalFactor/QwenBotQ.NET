using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using QwenBotQ.NET.Models;
using QwenBotQ.NET.Services.Interfaces;
using System.Text;

namespace QwenBotQ.NET.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _dbPath;
    private readonly string _dbName;
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<UserModel> _userCollection;

    public DatabaseService(string dbPath, string dbName)
    {
        _dbPath = dbPath;
        _dbName = dbName;

        ConventionRegistry.Register(
            "SnakeCaseConventions",
            new ConventionPack
            {
                new SnakeCaseElementNameConvention()
            },
            t => true
        );

        _client = new MongoClient(_dbPath);
        _database = _client.GetDatabase(_dbName);
        _userCollection = _database.GetCollection<UserModel>("User");
    }

    public async Task<UserModel?> GetUserAsync(string userId)
    {
         return await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
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