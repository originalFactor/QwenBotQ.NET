namespace QwenBotQ.SDK.Commands;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public string[] Triggers { get; }
    
    public CommandAttribute(string name, string description, params string[] triggers)
    {
        Name = name;
        Description = description;
        Triggers = triggers;
    }
}
