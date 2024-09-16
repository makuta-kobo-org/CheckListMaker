using CheckListMaker.Models;

namespace CheckListMaker.Services;

/// <summary> Azure Computer Visionのサービスのインターフェイス </summary>
internal interface IComputerVisionService
{
    /// <summary>
    /// パラメータの画像ファイルをComputer VisionでOCR処理し、
    /// CheckItemのListを生成して返す</summary>
    Task<CheckItems> GetCheckItems(string localFile);
}
