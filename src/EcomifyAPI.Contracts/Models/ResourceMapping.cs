namespace EcomifyAPI.Contracts.Models;

public class RoleMappingDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Composite { get; set; }
    public bool ClientRole { get; set; }
    public string ContainerId { get; set; } = string.Empty;
}

public class ResourceMappingDTO
{
    public string Id { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public List<RoleMappingDTO> Mappings { get; set; } = [];
}

public class ResourceMappingsResponseDTO
{
    public Dictionary<string, ResourceMappingDTO> ClientMappings { get; set; } = [];
}