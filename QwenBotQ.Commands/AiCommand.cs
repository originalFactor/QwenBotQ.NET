using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.DatabaseS;
using System.ClientModel;
using QwenBotQ.SDK.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace QwenBotQ.Commands;

[Command("AI回复", "使用AI回复", "ai", "ask")]
public class AiCommand : Command
{
    readonly OpenAIClient client;
    readonly Database database;
    readonly string defaultModel;
    readonly ILogger<AiCommand> logger;

    public AiCommand(IConfiguration configuration, Database database, ILogger<AiCommand> logger)
    {
        client = new OpenAIClient(
            new ApiKeyCredential(configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("Must specify an OpenAI Key.")),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(configuration["OpenAI:BaseUri"] ?? "https://api.openai.com/v1/"),
            }
        );
        this.database = database;
        defaultModel = configuration["OpenAI:DefaultModel"] ?? "gpt-4o-mini";
        this.logger = logger;
    }

    public override async Task ExecuteAsync(MessageContext context)
    {
        var user = await context.GetSenderUserAsync(database);
        if (user == null)
        {
            await context.Quick("喵喵看不见你诶，难过……");
            return;
        }
        if(user.Coins <= 0)
        {
            await context.Quick("喵喵没有钱了，付不起LLM的昂贵费用，请签到后再试！");
            return;
        }
        var prompt = await context.TrackRepliesAsync();
        long tokens = user.SystemPrompt.Length;
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(user.SystemPrompt)
        };
        foreach (var msg in prompt.Reverse())
        {
            var text = msg.Message.GetPlainText().Trim();
            tokens += text.Length;
            messages.Add(
                msg.Sender.UserId == context.Event!.SelfId
                ? new AssistantChatMessage(text)
                : new UserChatMessage(string.Join(" ", text.Split(" ")[1..]))
            );
        }
        var chat = client.GetChatClient(user.model ?? defaultModel);
        var r = await chat.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            Temperature = (float)user.Temprature,
            FrequencyPenalty = (float)user.FrequencyPenalty,
            PresencePenalty = (float)user.PresencePenalty
        });
        var resp = r.Value.Content.FirstOrDefault()?.Text;
        tokens += resp?.Length ?? 0;
        user.Coins -= Math.Max(1, tokens / 1000);
        await database.SaveUserAsync(user);
        logger.LogDebug($"""
            Messages: 
            {JsonConvert.SerializeObject(messages, Formatting.Indented)}
            """);
        await context.Quick(resp ?? "喵喵没有回复呢，可能是因为你问的问题太难了，或者喵喵还在学习中。请稍后再试试！");
    }
}
