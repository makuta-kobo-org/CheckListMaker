using CheckListMaker.Resources;

namespace CheckListMaker.Exceptions;

/// <summary> 使用許可が無い場合のException </summary>
internal class NoPermissionsException : ExceptionBase
{
    /// <summary> コンストラクタ </summary>
    public NoPermissionsException(string message)
        : base($"{AppResource.Exception_NoPermissions}{Environment.NewLine}Target: {message}")
    {
    }

    /// <summary> Gets 例外種別 </summary>
    public override ExceptionKind Kind => ExceptionKind.Error;
}
