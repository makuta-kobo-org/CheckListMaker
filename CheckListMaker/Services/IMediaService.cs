namespace CheckListMaker.Services;

/// <summary> MediaPickerのServiceのインターフェイス </summary>
internal interface IMediaService
{
    /// <summary> 画像撮影 </summary>
    Task<string> DoCapturePhoto();

    /// <summary> 画像選択 </summary>
    Task<string> DoPickPhoto();
}
