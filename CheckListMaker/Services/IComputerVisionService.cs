using CheckListMaker.Models;

namespace CheckListMaker.Services;

/// <summary>
/// Interface for Azure Computer Vision service.
/// </summary>
public interface IComputerVisionService
{
    /// <summary>
    /// Processes the specified image file using Azure Computer Vision OCR
    /// and generates a list of checklist items.
    /// </summary>
    /// <param name="localFile">The path to the local image file to process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated checklist.</returns>
    Task<CheckList> GetCheckItems(string localFile);
}
