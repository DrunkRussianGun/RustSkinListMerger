using System.Text.Json.Serialization;

namespace RustSkinListMerger.Dto;

public class SkinDto
{
	[JsonPropertyName("Item Shortname")]
	public string ItemShortName { get; set; } = null!;

	[JsonPropertyName("Permission")]
	public string Permission { get; set; } = null!;

	[JsonPropertyName("Skins")]
	public List<ulong> SkinIds { get; set; } = new();
}
