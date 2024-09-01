namespace CheckListMaker.Services;

/// <summary> MediaPickerのサービスクラス </summary>
internal sealed class MediaService : IMediaService
{
    /// <summary> 画像選択 </summary>
    public async Task<string> DoPickPhoto()
    {
        var photo = await MediaPicker.PickPhotoAsync();

        if (photo == null)
        {
            return null;
        }

        var photoPath = await LoadPhotoAsync(photo);

        Console.WriteLine($"PickPhotoAsync COMPLETED: {photoPath}");

        return photoPath;
    }

    /// <summary> 画像撮影 </summary>
    public async Task<string> DoCapturePhoto()
    {
        var photo = await MediaPicker.CapturePhotoAsync();

        if (photo == null)
        {
            return null;
        }

        var photoPath = await LoadPhotoAsync(photo);

        Console.WriteLine($"CapturePhotoAsync COMPLETED: {photoPath}");

        return photoPath;
    }

    private static async Task<string> LoadPhotoAsync(FileResult photo)
    {
        // canceled
        ArgumentNullException.ThrowIfNull(photo);

        // save the file into local storage
        var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        using (var stream = await photo.OpenReadAsync())
        using (var newStream = File.OpenWrite(newFile))
        {
            await stream.CopyToAsync(newStream);
        }

        return newFile;
    }
}
