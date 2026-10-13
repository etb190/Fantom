using System.Text.Json.Serialization;

namespace Fantom.Models;

/// <summary>
/// A trackable work task. Serialization format matches the existing
/// tasks.json exactly: { "id", "name", "color" }.
/// id is a GUID string, color is a hex string like "#2563eb".
/// </summary>
public class TaskItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#2563eb";
}
