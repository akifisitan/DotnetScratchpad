using System.Text.Json.Serialization;
using Scratchpad.Lib.Model;

namespace Scratchpad.Lib;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(UserCredentials))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
