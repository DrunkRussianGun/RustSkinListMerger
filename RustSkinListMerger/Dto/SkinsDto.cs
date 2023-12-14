using System.Text.Json.Serialization;

namespace RustSkinListMerger.Dto;

public class SkinsDto
{
	[JsonPropertyName("Skins")]
	public SkinDto[] Skins { get; set; } = Array.Empty<SkinDto>();
}
