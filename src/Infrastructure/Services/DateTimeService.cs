using Application.Common.Abstractions;

namespace Infrastructure.Services;

internal class DateTimeService : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
