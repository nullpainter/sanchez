using Newtonsoft.Json;

namespace Sanchez.Processing.Models.Configuration;

public record ImagePathConfiguration
{
    [JsonProperty(Required = Required.Always)]
    public required string Satellite { get; init; } 
   
    [JsonProperty(Required = Required.Always)]
    public required string Directory { get; init; }
}