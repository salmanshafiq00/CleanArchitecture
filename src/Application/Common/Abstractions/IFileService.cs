namespace Application.Common.Abstractions;

public interface IFileService
{

    /// <summary>
    /// Saves a stock count CSV file from a list of data
    /// </summary>
    /// <param name="data">List of data to save</param>
    /// <param name="directoryPath">Directory to save the file</param>
    /// <param name="headers">CSV headers</param>
    /// <param name="fileName">Name of the file to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full path or URL of the saved file</returns>
    Task<string> SaveCsvFileAsync<T>(
        List<T> data,
        string folderPath,
        string[]? headers = null,
        string? fileName = null,
        CancellationToken cancellationToken = default) where T : class;



    /// <summary>
    /// Saves a stock count CSV file from a stream
    /// </summary>
    /// <param name="dataStream">Stream containing CSV data</param>
    /// <param name="directoryPath">Directory to save the file</param>
    /// <param name="fileName">Name of the file to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full path or URL of the saved file</returns>
    Task<string> SaveStockCountCsvFileAsync(
        Stream dataStream,
        string directoryPath,
        string fileName,
        CancellationToken cancellationToken);
}
