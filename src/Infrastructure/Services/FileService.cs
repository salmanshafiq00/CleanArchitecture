using System.Text;
using Application.Common.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services;

internal sealed class FileService(
    IWebHostEnvironment environment,
    ILogger<FileService> logger) : IFileService
{
    public async Task<string> SaveCsvFileAsync<T>(
        List<T> data,
        string folderPath,
        string[]? headers = null,
        string? fileName = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Ensure absolute path
            var absoluteFolderPath = Path.Combine(environment.WebRootPath, folderPath);

            // Create directory if not exists
            Directory.CreateDirectory(absoluteFolderPath);

            // Generate filename if not provided
            fileName ??= $"{DateTime.Now:yyyyMMdd-HHmmssfff}.csv";
            var filePath = Path.Combine(absoluteFolderPath, fileName);

            // Get properties of the type and cache them
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .ToList();

            // Use StreamWriter for direct CSV writing
            await using (var writer = new StreamWriter(filePath, false, Encoding.UTF8, 4096))
            {
                // Write headers
                var headerLine = headers != null
                    ? string.Join(",", headers)
                    : string.Join(",", properties.Select(p => p.Name));
                await writer.WriteLineAsync(headerLine);

                // Write data rows
                foreach (var item in data)
                {
                    var rowValues = new string[properties.Count];
                    for (int col = 0; col < properties.Count; col++)
                    {
                        var value = properties[col].GetValue(item);
                        rowValues[col] = value?.ToString() ?? string.Empty;
                    }
                    var rowLine = string.Join(",", rowValues);
                    await writer.WriteLineAsync(rowLine);
                }
            }

            logger.LogInformation("CSV file saved: {filePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving CSV file");
            throw;
        }
    }


    public async Task<string> SaveStockCountCsvFileAsync(
        Stream dataStream,
        string directoryPath,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Ensure directory exists
            var fullDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), directoryPath);
            Directory.CreateDirectory(fullDirectoryPath);

            // Generate full file path
            var fullFilePath = Path.Combine(fullDirectoryPath, fileName);

            // Save the file
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await dataStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Return relative path for web access
            return Path.Combine("/", directoryPath, fileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving stock count CSV file: {FileName}", fileName);
            throw;
        }
    }
}
