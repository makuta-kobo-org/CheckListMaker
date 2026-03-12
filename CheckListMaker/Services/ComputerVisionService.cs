using Azure;
using Azure.AI.Vision.ImageAnalysis;
using CheckListMaker.Models;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker.Services;

/// <summary> Azure Computer Vision のサービスクラス </summary>
internal sealed class ComputerVisionService : IComputerVisionService
{
    private static readonly object _lockObject = new ();
    private static ComputerVisionService _instance = null;
    private readonly CVConstants _constants;

    private ComputerVisionService(IConfiguration config) =>
        _constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();

    /// <summary> Instanceを返す </summary>
    public static ComputerVisionService GetInstance(IConfiguration config)
    {
        if (_instance == null)
        {
            lock (_lockObject)
            {
                _instance ??= new ComputerVisionService(config);
            }
        }

        return _instance;
    }

    /// <summary>
    /// パラメータの画像ファイルをComputer VisionでOCR処理し、CheckItemのListを生成して返す
    /// </summary>
    public async Task<CheckList> GetCheckItems(string localFile)
    {
        var client = CreateImageAnalysisClient();
        var result = await AnalyzeImageAsync(client, localFile);

        return ExtractCheckItems(result);
    }

    private ImageAnalysisClient CreateImageAnalysisClient() =>
        new (
            new Uri(_constants.EndPoint),
            new AzureKeyCredential(_constants.Key));

    private async Task<ImageAnalysisResult> AnalyzeImageAsync(ImageAnalysisClient client, string localFile)
    {
        using var imageStream = File.OpenRead(localFile);
        var imageData = BinaryData.FromStream(imageStream);

        return await client.AnalyzeAsync(
            imageData,
            VisualFeatures.Read,
            new ImageAnalysisOptions { Language = "ja" });
    }

    private CheckList ExtractCheckItems(ImageAnalysisResult result)
    {
        var items = new CheckList();

        if (result.Read?.Blocks == null)
        {
            return items;
        }

        foreach (var block in result.Read.Blocks)
        {
            foreach (var line in block.Lines)
            {
                var text = line.Text.StartsWith('·')
                    ? line.Text.Remove(0, 1).Trim()
                    : line.Text.Trim();

                items.Items.Add(new CheckItem { ItemText = text });
            }
        }

        return items;
    }
}
