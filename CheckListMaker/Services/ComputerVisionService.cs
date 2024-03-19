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
    private static IComputerVisionService _instance = null;
    private readonly CVConstants _constants;

    private ComputerVisionService(IConfiguration config) =>
        _constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();

    /// <summary> Instanceを返す </summary>
    public static IComputerVisionService GetInstance(IConfiguration config)
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
    /// パラメータの画像ファイルをComputer VisionでOCR処理し、
    /// CheckItemのListを生成して返す</summary>
    public async Task<ICollection<CheckItem>> GetCheckItems(string localFile)
    {
        using var client = new ComputerVisionClient(
            new ApiKeyServiceClientCredentials(_constants.Key))
        {
            Endpoint = _constants.EndPoint,
        };

        Console.WriteLine("[ComputerVisionService] Read file from url");

        // Read text from URL
        var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile));

        // After the request, get the operation location (operation ID)
        string operationLocation = textHeaders.OperationLocation;

        var operationId = operationLocation[^NumberOfCharsInOperationId..];

        // Extract the text
        ReadOperationResult results;

        Console.WriteLine($"[ComputerVisionService] Reading text from  {Path.GetFileName(localFile)}...");

        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
        }
        while (results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted);

        Console.WriteLine("[ComputerVisionService] Read text from result");

        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        var items = new List<CheckItem>();

        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                var text = line.Text[0] == '·'
                    ? line.Text.Remove(0, 1).Trim()
                    : line.Text.Trim();

                items.Add(new CheckItem() { ItemText = text });
            }
        }

        return items;
    }
}
