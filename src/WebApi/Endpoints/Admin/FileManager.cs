using Application.Common.Models;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Admin;

public class FileManager : EndpointGroupBase
{
    private const string BaseDirectory = "Resources/images";

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("Upload", Upload)
            .WithName("Upload")
            .Produces<FileResponse[]>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapDelete("RemoveFile", RemoveFile)
            .WithName("RemoveFile")
            .Produces<FileResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // Multiple file deletion
        group.MapDelete("/RemoveMultiple", RemoveMultipleFiles)
            .WithName("RemoveMultipleFiles")
            .Produces<RemoveFilesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("{fileName}", GetImage)
             .WithName("GetImage")
             .AllowAnonymous()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status404NotFound)
             .Produces(StatusCodes.Status500InternalServerError);

    }


    private async Task<IResult> Upload(IHttpContextAccessor context)
    {
        try
        {
            var fileNames = context.HttpContext.Request.Headers["X-File-Names"].ToString().Split(',');
            var fileSizes = context.HttpContext.Request.Headers["X-File-Sizes"].ToString().Split(',').Select(long.Parse).ToArray();
            var fileTypes = context.HttpContext.Request.Headers["X-File-Types"].ToString().Split(',');
            var location = context.HttpContext.Request.Headers["X-File-Location"].ToString();

            if (fileNames.Length != fileSizes.Length || fileNames.Length != fileTypes.Length)
                return Results.BadRequest("File metadata is inconsistent.");

            location = string.IsNullOrWhiteSpace(location) ? "files" : location;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Resources", location);
            Directory.CreateDirectory(uploadsFolder);

            using var reader = new BinaryReader(context.HttpContext.Request.Body);
            var totalBytes = context.HttpContext.Request.ContentLength ?? 0;
            var fileData = await reader.ReadBytesAsync((int)totalBytes);

            var fileResponses = new List<FileResponse>();
            var currentPosition = 0;

            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = fileNames[i]?.Replace(' ', '_');
                var fileSize = fileSizes[i];
                var fileType = fileTypes[i];

                if (fileData.Length < currentPosition + fileSize)
                    return Results.BadRequest("File data size mismatch.");

                var fileBytes = fileData[currentPosition..(currentPosition + (int)fileSize)];
                currentPosition += (int)fileSize;

                var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await File.WriteAllBytesAsync(filePath, fileBytes);

                var relativePath = Path.Combine("/Resources", location, uniqueFileName).Replace("\\", "/");
                fileResponses.Add(new FileResponse(relativePath));
            }

            return Results.Ok(fileResponses);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error uploading files: {ex.Message}");
        }
    }

    //public record RemoveFileRequest(string RelativePath);

    public async Task<IResult> RemoveFile([FromBody] RemoveFileRequest removeFileReq)
    {
        // Validate the input
        if (string.IsNullOrWhiteSpace(removeFileReq.RelativePath) || removeFileReq.RelativePath.Contains(".."))
        {
            return Results.BadRequest("Invalid file path.");
        }

        try
        {
            // Convert the relative path to an absolute path
            string absolutePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), removeFileReq.RelativePath.TrimStart(Path.DirectorySeparatorChar, '/')));

            //if (!absolutePath.StartsWith(removeFileReq.RelativePath))
            //{
            //    return Results.BadRequest("Access denied: Path is outside of allowed directory");
            //}

            if (!File.Exists(absolutePath))
            {
                return Results.NotFound("File not found.");
            }

            // Delete the file
            await Task.Run(() => File.Delete(absolutePath));
            return Results.Ok("File removed successfully.");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access denied. The file could not be deleted."
            );
        }
        catch (Exception ex)
        {
            // Log the exception for debugging
            Console.Error.WriteLine($"Error deleting file: {ex.Message}\n{ex.StackTrace}");

            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal server error"
            );
        }
    }

    private static async Task<IResult> RemoveMultipleFiles(
       [FromBody] RemoveFilesRequest request,
       IConfiguration config,
       ILogger<Program> logger)
    {
        // Retrieve the base file storage path
        var baseFilePath = config["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Validate input
        if (request.RelativePaths == null || !request.RelativePaths.Any())
        {
            return Results.BadRequest("No files specified for deletion");
        }

        var results = new List<FileResponse>();

        // Iterate over each file path
        foreach (var relativePath in request.RelativePaths)
        {
            try
            {
                // Validate the file path
                if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains(".."))
                {
                    results.Add(new FileResponse(relativePath)
                    {
                        IsSucceed = false,
                        Message = "Invalid file path"
                    });
                    continue;
                }

                // Construct the absolute file path
                string absolutePath = Path.GetFullPath(
                    Path.Combine(baseFilePath, relativePath.TrimStart(Path.DirectorySeparatorChar, '/')));

                // Ensure the absolute path is within the allowed directory
                if (!absolutePath.StartsWith(baseFilePath, StringComparison.Ordinal))
                {
                    results.Add(new FileResponse(relativePath)
                    {
                        IsSucceed = false,
                        Message = "Access denied: Path is outside of allowed directory"
                    });
                    continue;
                }

                // Check if the file exists
                if (!File.Exists(absolutePath))
                {
                    results.Add(new FileResponse(relativePath)
                    {
                        IsSucceed = false,
                        Message = "File not found"
                    });
                    continue;
                }

                // Perform the file deletion
                await Task.Run(() => File.Delete(absolutePath));
                results.Add(new FileResponse(relativePath)
                {
                    IsSucceed = true,
                    Message = "File deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Log and handle unauthorized access attempts
                logger.LogWarning($"Unauthorized access attempt to delete file: {relativePath}");
                results.Add(new FileResponse(relativePath)
                {
                    IsSucceed = false,
                    Message = "Access denied"
                });
            }
            catch (Exception ex)
            {
                // Log and handle unexpected errors
                logger.LogError(ex, $"Error deleting file: {relativePath}");
                results.Add(new FileResponse(relativePath)
                {
                    IsSucceed = false,
                    Message = "Internal server error"
                });
            }
        }

        // Determine overall success and generate a response message
        var allSuccessful = results.All(r => r.IsSucceed);
        var responseMessage = allSuccessful
            ? "All files deleted successfully"
            : "Some files could not be deleted";

        // Return the result
        return Results.Ok(new RemoveFilesResponse
        {
            Success = allSuccessful,
            Message = responseMessage,
            FileResponses = results
        });
    }

    private async Task<IResult> GetImage(string fileName)
    {
        // Validate and sanitize the fileName input
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
        {
            return Results.BadRequest("Invalid file path.");
        }
        var filePath = Path.Combine(BaseDirectory, fileName);

        if (!File.Exists(filePath))
        {
            return Results.NotFound("File not found.");
        }
        try
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = fileExtension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return Results.File(memory, contentType);
        }
        catch
        {
            // Log exception details if necessary
            return Results.Problem("An error occurred while processing your request.");
        }
    }
}
