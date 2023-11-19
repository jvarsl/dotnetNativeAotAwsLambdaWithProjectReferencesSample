using System.Text.Json.Serialization;

namespace Models;

public class Customer
{
    [JsonPropertyName("pk")]
    public string Pk { get; set; } = default!;

    [JsonPropertyName("sk")]
    public string Sk { get; set; } = default!;

    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;
}

