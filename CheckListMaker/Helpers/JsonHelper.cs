using System.Text.Json;
using CheckListMaker.Models;

namespace CheckListMaker.Helpers;

/// <summary> Jsonファイル操作のヘルパークラス </summary>
internal class JsonHelper
{
    private const string JsonFileName = "checkItems.json";

    /// <summary> JsonからCheckItemsのコレクションを取得し返す </summary>
    internal static IEnumerable<CheckItems> ReadCheckItemsList()
    {
        var items = new List<CheckItems>();

        var jsonPath = Path.Combine(
            FileSystem.Current.AppDataDirectory, JsonFileName);

        if (!File.Exists(jsonPath))
        {
            return items;
        }

        var json = File.ReadAllText(jsonPath);

        return JsonSerializer.Deserialize<List<CheckItems>>(json);
    }
}
