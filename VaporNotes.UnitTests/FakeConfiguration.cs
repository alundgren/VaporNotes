using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace VaporNotes.UnitTests;

internal class FakeConfiguration : IConfiguration
{
    public Dictionary<string, string?> values = new();
    
    public string? this[string key] { get => values.GetValueOrDefault(key); set => values[key] = value; }

    public FakeConfiguration Set(string key, string value)
    {
        values[key] = value;
        return this;
    }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

    public IChangeToken GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
        throw new NotImplementedException();
    }
}
