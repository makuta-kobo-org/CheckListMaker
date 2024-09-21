using CheckListMaker.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker.Services;

/// <summary> Azure Computer Vision のサービスクラス </summary>
internal sealed class ComputerVisionService : IComputerVisionService
{
    private const int NumberOfCharsInOperationId = 36;
    private static readonly object _lockObject = new();
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
    public async Task<CheckItems> GetCheckItems(string localFile)
    {
        using var client = CreateComputerVisionClient();
        var operationId = await StartOcrOperation(client, localFile);
        var results = await WaitForOcrResults(client, operationId);

        return ExtractCheckItems(results.AnalyzeResult);
    }

    private ComputerVisionClient CreateComputerVisionClient() =>
    new(new ApiKeyServiceClientCredentials(_constants.Key))
    {
        Endpoint = _constants.EndPoint,
    };

    private async Task<string> StartOcrOperation(ComputerVisionClient client, string localFile)
    {
        var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile));
        return textHeaders.OperationLocation[^NumberOfCharsInOperationId..];
    }

    private async Task<ReadOperationResult> WaitForOcrResults(ComputerVisionClient client, string operationId)
    {
        ReadOperationResult results;
        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
            await Task.Delay(1000); // 1秒ごとにステータス確認
        }
        while (results.Status == OperationStatusCodes.Running ||
               results.Status == OperationStatusCodes.NotStarted);

        return results;
    }

    private CheckItems ExtractCheckItems(AnalyzeResults analyzeResults)
    {
        var items = new CheckItems();

        foreach (var page in analyzeResults.ReadResults)
        {
            foreach (var line in page.Lines)
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
