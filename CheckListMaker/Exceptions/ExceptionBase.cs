namespace CheckListMaker.Exceptions;

/// <summary> Exception の基本クラス </summary>
internal abstract class ExceptionBase : Exception
{
    /// <summary> コンストラクタ </summary>
    /// <param name="message">例外メッセージ</param>
    public ExceptionBase(string message)
        : base(message)
    {
    }

    /// <summary> 例外とメッセージを受けるコンストラクタ </summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="exception">Exception</param>
    public ExceptionBase(string message, Exception exception)
        : base(message, exception)
    {
    }

    /// <summary> 例外種別のEnum </summary>
    public enum ExceptionKind
    {
        /// <summary> Info Exception </summary>
        Info,

        /// <summary> Waning Exception </summary>
        Warning,

        /// <summary> Error Exception </summary>
        Error,
    }

    /// <summary> Gets 例外種別Enumィ </summary>
    public abstract ExceptionKind Kind { get; }
}
