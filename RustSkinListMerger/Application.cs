﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using RustSkinListMerger.Dto;

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
