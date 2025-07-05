using Microsoft.Extensions.Configuration;

namespace QwenBotQ.NET
{
    internal class ConfigModel
    {
        public string OneBotServer { get; set; } = "ws://127.0.0.1:3001/ws";
        public string? OneBotToken { get; set; } = "napcatqq";
        public string MongoUri { get; set; } = "mongodb://127.0.0.1:27017";
        public string MongoDbName { get; set; } = "aioBot";

        public ConfigModel()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            config.Bind(this);
        }
    }
}
