using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using RustSkinListMerger.Dto;

const int defaultSkinId = 0;

if (args.Length < 1)
{
	Console.Error.WriteLine("Usage: RustSkinListMerger [input file name]... <output file name>");
	return 1;
}

var inputFileNames = args[..^1];
var mergedSkinsMap = MergeSkins(inputFileNames);
var mergedSkinsDto = ToDto(mergedSkinsMap);
var outputFileName = args[^1];
WriteSkins(mergedSkinsDto, outputFileName);
DetectSameSkinIdsInDifferentItems(mergedSkinsDto);

return 0;

static SkinsDto ReadSkins(string fileName)
{
	using var inputFile = new FileStream(
		fileName,
		FileMode.Open,
		FileAccess.Read,
		FileShare.ReadWrite | FileShare.Delete);
	return JsonSerializer.Deserialize<SkinsDto>(
			inputFile,
			new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				AllowTrailingCommas = true,
				NumberHandling = JsonNumberHandling.AllowReadingFromString
			})
		?? new SkinsDto();
}

static Dictionary<string, SkinDto> MergeSkins(string[] inputFileNames)
{
	var mergedSkinsMap = new Dictionary<string, SkinDto>();

	foreach (var inputFileName in inputFileNames)
	{
		Console.WriteLine($"Processing file {inputFileName}");
		var inputSkins = ReadSkins(inputFileName);
		foreach (var inputSkin in inputSkins.Skins)
		{
			if (!mergedSkinsMap.TryGetValue(inputSkin.ItemShortName, out var mergedSkin))
			{
				inputSkin.SkinIds.Add(defaultSkinId);
				mergedSkinsMap.Add(inputSkin.ItemShortName, inputSkin);
				continue;
			}

			mergedSkin.SkinIds.AddRange(inputSkin.SkinIds);
		}
	}

	foreach (var skin in mergedSkinsMap.Values)
		skin.SkinIds = skin.SkinIds.Distinct().OrderBy(x => x).ToList();

	return mergedSkinsMap;
}

static SkinsDto ToDto(Dictionary<string, SkinDto> skinsMap)
	=> new()
	{
		Skins = skinsMap.Values
			.OrderBy(x => x.ItemShortName)
			.ToArray()
	};

static void WriteSkins(SkinsDto skinsDto, string fileName)
{
	Console.WriteLine($"Writing result to file {fileName}");
	using var outputFile = new FileStream(
		fileName,
		FileMode.CreateNew,
		FileAccess.Write,
		FileShare.Read);
	JsonSerializer.Serialize(
		outputFile,
		skinsDto,
		new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			WriteIndented = true
		});
}

static void DetectSameSkinIdsInDifferentItems(SkinsDto skins)
{
	var skinIdMap = skins.Skins
		.SelectMany(
			skin => skin.SkinIds
				// Exclude default skin as it can normally be defined for multiple items
				.Where(skinId => skinId != defaultSkinId)
				.Select(skinId => (skin.ItemShortName, SkinId: skinId)))
		.ToLookup(x => x.SkinId, x => x.ItemShortName);
	foreach (var itemShortNames in skinIdMap)
		if (itemShortNames.Count() > 1)
			Console.Error.WriteLine(
				$"Warning: skin ID {itemShortNames.Key} is defined for multiple items:"
				+ Environment.NewLine
				+ string.Join(Environment.NewLine, itemShortNames));
}
