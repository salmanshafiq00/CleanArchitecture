namespace Application.Common.DapperQueries;

public record DapperPaginatedData
{
    public int Offset { get; set; } = 0;
    public int Next { get; set; } = 10;
}
