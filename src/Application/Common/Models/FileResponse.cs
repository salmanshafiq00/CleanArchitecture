namespace Application.Common.Models;

public record FileResponse(string FilePath)
{
    public string Message { get; set; } = string.Empty;
    public bool IsSucceed { get; set; }
}

public record RemoveFileRequest(string RelativePath)
{

}

public record RemoveFilesRequest(string[] RelativePaths)
{

}

public record RemoveFilesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<FileResponse> FileResponses { get; set; } = [];
}
