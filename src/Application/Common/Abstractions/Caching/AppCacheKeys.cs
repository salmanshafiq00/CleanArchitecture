namespace Application.Common.Abstractions.Caching;

public static class AppCacheKeys
{
    public const string Lookup = nameof(Lookup);
    // This one was already using colon, keeping it consistent
    public const string Lookup_All_SelectList = "Lookup:All:SelectList";

    public const string LookupDetail = nameof(LookupDetail);
    public const string LookupDetail_All_SelectList = "LookupDetail:All:SelectList";

    #region Admin
    public const string AppUser = nameof(AppUser);
    public const string AppUser_Select_List = "AppUser:SelectList";
    public const string Role = nameof(Role);
    public const string Role_Permissions = "Role:Permissions";
    public const string Role_All_SelectList = "Role:SelectList";
    public const string AppMenu = nameof(AppMenu);
    public const string AppMenu_All_SelectList = "AppMenu:SelectList";
    public const string AppMenu_Parent_SelectList = "AppMenu:Parent:SelectList";
    public const string AppMenu_Tree_SelectList = "AppMenu:Tree:SelectList";
    public const string AppMenu_Sidebar_Tree_List = "AppMenu:Sidebar:Tree:List";
    public const string AppPage = nameof(AppPage);
    public const string AppPage_All_SelectList = "AppPage:SelectList";
    public const string AppNotification = nameof(AppNotification);

    #endregion

    #region Products
    public const string Category = nameof(Category);
    public const string Category_All_SelectList = "Category:SelectList";
    public const string Product = nameof(Product);
    public const string Product_All_SelectList = "Product:SelectList";
    public const string Product_Paginated = "Product:Paginated";
    public const string Warehouse = nameof(Warehouse);
    public const string Warehouse_All_SelectList = "Warehouse:SelectList";
    public const string Brand = nameof(Brand);
    public const string Brand_All_SelectList = "Brand:SelectList";
    public const string Brand_All_Tree_SelectList = "Brand:Tree:SelectList";
    public const string Unit = nameof(Unit);
    public const string Unit_All_SelectList = "Unit:SelectList";
    public const string Tax = nameof(Tax);
    public const string Tax_All_SelectList = "Tax:SelectList";
    public const string ProductAdjustment = nameof(ProductAdjustment);
    public const string ProductAdjustment_All_SelectList = "ProductAdjustment:SelectList";
    public const string CountStock = nameof(CountStock);
    #endregion

    #region Product Transfers
    public const string ProductTransfer = nameof(ProductTransfer);
    #endregion

    #region Stakeholders
    public const string Customer = nameof(Customer);
    public const string Customer_All_SelectList = "Customer:SelectList";
    public const string CustomerGroup = nameof(CustomerGroup);
    public const string CustomerGroup_All_SelectList = "CustomerGroup:SelectList";
    public const string Supplier = nameof(Supplier);
    public const string Supplier_All_SelectList = "Supplier:SelectList";
    #endregion

    #region Purchase
    public const string Purchase = nameof(Purchase);
    public const string PurchasePayment = nameof(PurchasePayment);
    public const string PurchasePayment_PurchaseId = "PurchasePayment:PurchaseId";
    public const string PurchaseReturn = nameof(PurchaseReturn);
    public const string PurchaseReturnPayment = nameof(PurchaseReturnPayment);
    #endregion

    #region Sales
    public const string Sale = nameof(Sale);
    public const string SalePayment = nameof(SalePayment);
    public const string Courier = nameof(Courier);
    public const string Coupon = nameof(Coupon);
    public const string GiftCard = nameof(GiftCard);
    public const string SaleReturn = nameof(SaleReturn);
    public const string SaleReturnPayment = nameof(SaleReturnPayment);
    public const string PosGet = nameof(PosGet);
    #endregion

    #region Accounting
    public const string Account = nameof(Account);
    public const string Account_All_SelectList = "Account:SelectList";
    public const string MoneyTransfer = nameof(MoneyTransfer);
    public const string MoneyTransfer_All_SelectList = "MoneyTransfer:SelectList";
    public const string Expense = nameof(Expense);
    public const string Expense_All_SelectList = "Expense:SelectList";
    #endregion

    #region Quotations
    public const string Quotation = nameof(Quotation);
    #endregion

    #region HRM
    public const string Department = nameof(Department);
    public const string Department_All_SelectList = "Department:SelectList";
    public const string Designation = nameof(Designation);
    public const string Designation_All_SelectList = "Designation:SelectList";
    public const string Employee = nameof(Employee);
    public const string Employee_All_SelectList = "Employee:SelectList";
    public const string WorkingShift = nameof(WorkingShift);
    public const string WorkingShift_All_SelectList = "WorkingShift:SelectList";
    public const string Attendance = nameof(Attendance);
    public const string LeaveRequest = nameof(LeaveRequest);
    public const string LeaveType = nameof(LeaveType);
    public const string Holiday = nameof(Holiday);
    #endregion

    #region Settings
    public const string CompanyInfo = nameof(CompanyInfo);
    #endregion
}
