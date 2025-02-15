using Dapper;
using System.Data;

namespace Infrastructure.Persistence.TypeHandlers;

public class DapperSqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly time)
    {
        // Convert TimeOnly to TimeSpan for storage
        parameter.Value = time.ToTimeSpan();
    }

    public override TimeOnly Parse(object value)
    {
        // Convert stored TimeSpan back to TimeOnly
        return TimeOnly.FromTimeSpan((TimeSpan)value);
    }
}

