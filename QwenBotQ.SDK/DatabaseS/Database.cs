using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using QwenBotQ.SDK.Models.Database;
using System.Text;

namespace QwenBotQ.SDK.DatabaseS;

public class Database
{
    private readonly IMongoCollection<UserModel> _userCollection;
    private readonly ILogger<Database> _logger;
    
    public Database(string connectionString, string databaseName, ILogger<Database> logger)
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

    public async Task<UserModel> GetUserOrCreateAsync(string userId, string nickname)
    {
        try
        {
            var user = await GetUserAsync(userId);
            if (user == null)
            {
                user = new UserModel
                {
                    Id = userId,
                    Nick = nickname
                };
                await SaveUserAsync(user);
                _logger.LogInformation($"Created new user {user.Id} with nickname {user.Nick}");
            }
            else if (user.Nick != nickname)
            {
                user.Nick = nickname;
                await SaveUserAsync(user);
                _logger.LogInformation($"Updated user {user.Id} with new nickname {user.Nick}");
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting or creating user {userId}");
            throw;
        }
    }

    // Get multiple users by customized conditions, orders, and limits.
    public async Task<List<UserModel>> GetUsersAsync(
        FilterDefinition<UserModel>? filter = null,
        SortDefinition<UserModel>? sort = null,
        int? limit = null)
    {
        try
        {
            var query = _userCollection.Find(filter);
            if (sort != null)
            {
                query = query.Sort(sort);
            }
            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }
            var users = await query.ToListAsync();
            
            _logger.LogDebug($"Retrieved {users.Count} users with specified conditions");
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with specified conditions");
            throw;
        }
    }

    public async Task<DateTime> BindUserAsync(UserModel user1, UserModel user2)
    {
        var expire = DateTime.Today.AddDays(1);

        user1.Binded = new BindedModel
        {
            Ident = user2.Id,
            Expire = expire
        };
        user1.Binded = new BindedModel
        {
            Ident = user2.Id,
            Expire = expire
        };

        await SaveUserAsync(user1);
        await SaveUserAsync(user2);
        return expire;
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