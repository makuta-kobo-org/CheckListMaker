using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public override ExceptionKind Kind => ExceptionKind.Error;
}
