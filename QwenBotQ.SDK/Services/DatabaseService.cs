using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using QwenBotQ.SDK.Models;
using System.Text;

namespace QwenBotQ.SDK.Services;

public class DatabaseService : IDatabaseService
{
    private readonly IMongoCollection<UserModel> _userCollection;
    private readonly ILogger<DatabaseService> _logger;
    
    public DatabaseService(string connectionString, string databaseName, ILogger<DatabaseService> logger)
    {
        _logger = logger;
        
        try
        {
            // 注册snake_case字段名约定
            ConventionRegistry.Register(
                "SnakeCaseConventions",
                new ConventionPack
                {
                    new SnakeCaseElementNameConvention()
                },
                t => true
            );
            
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _userCollection = database.GetCollection<UserModel>("User");
            
            _logger.LogInformation("Database service initialized successfully with snake_case conventions.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database service");
            throw;
        }
    }
    
    public async Task<UserModel?> GetUserAsync(string userId)
    {
        try
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
            
            _logger.LogDebug($"Retrieved user {userId}: {(user != null ? user.Nick : "not found")}");
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user {userId}");
            return null;
        }
    }
    
    public async Task SaveUserAsync(UserModel user)
    {
        try
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            
            var filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
            var options = new ReplaceOptions { IsUpsert = true };
            
            await _userCollection.ReplaceOneAsync(filter, user, options);
            
            _logger.LogDebug($"Saved user {user.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving user {user.Id}");
            throw;
        }
    }
}

/// <summary>
/// MongoDB字段名snake_case约定
/// </summary>
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