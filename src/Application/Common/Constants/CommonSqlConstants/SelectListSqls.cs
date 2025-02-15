namespace Application.Common.Constants.CommonSqlConstants;

public static class SelectListSqls
{
    #region Common

    public const string GetLookupSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Lookups l
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string GetLookupParentSelectListSql = """
        SELECT l.Id, l.Name 
        FROM dbo.Lookups l
        INNER JOIN dbo.Lookups l2 ON l2.ParentId = l.Id
        WHERE 1 = 1
        ORDER BY l.Name
        """;

    public const string GetLookupDetailSelectListSql = """
        SELECT Id, Name 
        FROM dbo.LookupDetails ld
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string GetLookupDetailParentSelectListSql = """
        SELECT Distinct l.Id, l.Name 
        FROM dbo.LookupDetails l
        INNER JOIN dbo.LookupDetails l2 ON l2.ParentId = l.Id
        WHERE 1 = 1
        ORDER BY l.Name
        """;

    public const string GetLookupDetailSelectListByDevCodeSql = """
        SELECT ld.Id, ld.Name 
        FROM dbo.Lookups l
        INNER JOIN dbo.LookupDetails ld ON ld.LookupId = l.Id
        WHERE 1 = 1
        AND l.DevCode = @DevCode
        ORDER BY ld.Name
        """;

    public const string LookupDetailNameKeySelectListByDevCodeSql = """
        SELECT ld.Name Id, ld.Name 
        FROM dbo.Lookups l
        INNER JOIN dbo.LookupDetails ld ON ld.LookupId = l.Id
        WHERE 1 = 1
        AND l.DevCode = @DevCode
        ORDER BY ld.Name
        """;

    #endregion

    #region Admin

    public const string GetRoleSelectListSql = """
        SELECT Id, Name 
        FROM [identity].Roles r
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string GetRoleByNameSelectListSql = """
        SELECT Name AS Id, Name 
        FROM [identity].Roles r
        WHERE 1 = 1
        ORDER BY Name
        """;


    public const string GetAppMenuSelectListSql = """
        SELECT Id, Label AS Name
        FROM [dbo].AppMenus 
        WHERE 1 = 1
        ORDER BY Label
        """;

    public const string GetParentAppMenuSelectListSql = """
        SELECT DISTINCT m.Id, m.Label AS Name
        FROM [dbo].AppMenus m
        INNER JOIN [dbo].AppMenus m2 ON m2.ParentId = m.Id
        WHERE 1 = 1
        ORDER BY m.Label
        """;

    #endregion

    #region Products

    public const string CategorySelectListSql = """
        SELECT Id, Name 
        FROM dbo.Categories c
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string CategoryParentSelectListSql = """
        SELECT Distinct c.Id, c.Name 
        FROM dbo.Categories c
        INNER JOIN dbo.Categories c2 ON c2.ParentId = c.Id
        WHERE 1 = 1
        ORDER BY c.Name
        """;

    public const string BrandsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Brands t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string TaxesSelectListSql = """
        SELECT Rate AS Id, Name 
        FROM dbo.Taxes t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string UnitsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Units t
        WHERE 1 = 1
        AND BaseUnitId IS NULL
        ORDER BY Name
        """;

    public const string UnitWithOperatorSelectListSql = """
        SELECT Id, Name, BaseUnit, Operator, OperatorValue
        FROM dbo.Units t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string WarehouseSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Warehouses c
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string ProductsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Products t
        WHERE 1 = 1
        ORDER BY Name
        """;


    #endregion

    #region Stakeholders
    public const string GetCustomerSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Customers c
        WHERE 1 = 1
        ORDER BY Name
        """;
    public const string GetSupplierSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Suppliers c
        WHERE 1 = 1
        ORDER BY Name
        """;
    #endregion

    #region Accounting
    public const string AccountsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Accounts t
        WHERE 1 = 1
        ORDER BY Name
        """;
    #endregion

    #region HRM
    public const string DepartmentsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Departments t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string DesignationsSelectListSql = """
        SELECT Id, Name 
        FROM dbo.Designations t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string WorkingShiftsSelectListSql = """
        SELECT Id, ShiftName Name
        FROM dbo.WorkingShifts t
        WHERE 1 = 1
        ORDER BY Name
        """;

    public const string EmployeesSelectListSql = """
        SELECT Id, Concat(FirstName, ' ', LastName) Name
        FROM dbo.Employees t
        WHERE 1 = 1
        ORDER BY Name
        """;


    #endregion
}
