namespace CheckListMaker.Services;

/// <summary>
/// Interface for MediaPicker service.
/// Provides methods for capturing and selecting photos.
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Captures a photo using the device's camera.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the file path of the captured photo.
    /// </returns>
    Task<string> DoCapturePhoto();

    /// <summary>
    /// Selects a photo from the device's gallery.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the file path of the selected photo.
    /// </returns>
    Task<string> DoPickPhoto();
}
