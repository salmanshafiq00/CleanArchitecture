using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using ClosedXML.Excel;
using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Setups.LookupDetails.Commands;


public record CreateLookupDetailFromExcelCommand(
    IFormFile File) : ICacheInvalidatorCommand<int>
{
    public string[] CacheKeys => [AppCacheKeys.LookupDetail];
}

internal sealed class CreateLookupDetailFromExcelCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<CreateLookupDetailFromExcelCommand, int>
{
    public async Task<Result<int>> Handle(CreateLookupDetailFromExcelCommand request, CancellationToken cancellationToken)
    {
        var items = await ProcessUploadFile(request.File);

        var entities = new List<LookupDetail>();

        foreach (var item in items)
        {
            var lookupId = await dbContext.Lookups
                .AsNoTracking()
                .Where(x => x.Name.ToLower() == item.LookupName.ToLower())
                .Select(x => x.Id)
                .SingleOrDefaultAsync();

            if (lookupId.IsNullOrEmpty()) continue;

            Guid? parentId = null;
            if (!string.IsNullOrEmpty(item.ParentName))
            {
                parentId = await dbContext.Lookups
                    .AsNoTracking()
                    .Where(x => x.Name.ToLower() == item.ParentName.ToLower())
                    .Select(x => x.Id)
                    .SingleOrDefaultAsync();
            }

            entities.Add(new LookupDetail
            {
                Code = item.Code,
                Name = item.Name,
                ParentId = parentId,
                LookupId = lookupId,
                Description = item.Description,
                Status = !string.IsNullOrEmpty(item.Status) && item.Status == "1"
            });

        }

        dbContext.LookupDetails.AddRange(entities);
        var affectedRow = await dbContext.SaveChangesAsync(cancellationToken);

        return affectedRow;
    }

    private static async Task<List<LookupDetailExcelModel>> ProcessUploadFile(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var wordbook = new XLWorkbook(stream);
        var worksheet = wordbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed();

        var lookupDetails = new List<LookupDetailExcelModel>();

        foreach (var row in rows.Skip(1))
        {

            lookupDetails.Add(new LookupDetailExcelModel
            {
                Code = row.Cell(1).GetValue<string>(),
                Name = row.Cell(2).GetValue<string>(),
                ParentName = row.Cell(3).GetValue<string>(),
                LookupName = row.Cell(4).GetValue<string>(),
                Description = row.Cell(5).GetValue<string>(),
                Status = row.Cell(6).GetValue<string>()
            });
        }

        return lookupDetails;

    }

    private sealed class LookupDetailExcelModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public string? ParentName { get; set; }
        public string LookupName { get; set; }
    }
}
