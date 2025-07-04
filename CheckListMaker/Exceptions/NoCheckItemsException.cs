using CheckListMaker.Resources;

namespace CheckListMaker.Exceptions;

/// <summary> AI Visionの戻り値が0またはnullの場合のException </summary>
internal class NoCheckItemsException : ExceptionBase
{
    /// <summary> コンストラクタ </summary>
    public NoCheckItemsException()
        : base($"{AppResource.Exception_NoCheckItems}")
    {
    }

    /// <summary> Gets 例外種別 </summary>
    public override ExceptionKind Kind => ExceptionKind.Error;
}
