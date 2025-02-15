using Dapper;
using System.Data;

namespace Infrastructure.Persistence.TypeHandlers;

public class DapperSqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly date)
    {
        // Convert DateOnly to DateTime for storage
        parameter.Value = date.ToDateTime(new TimeOnly(0, 0));
    }

    public override DateOnly Parse(object value)
    {
        // Convert stored DateTime back to DateOnly
        return DateOnly.FromDateTime((DateTime)value);
    }
}


