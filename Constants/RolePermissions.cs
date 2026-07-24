namespace POS.API.Constants;

public static class RolePermissions
{
    private static readonly string[] AdminPermissions =
    [
        Permissions.Sales.View, Permissions.Sales.Create, Permissions.Sales.AddItems,
        Permissions.Sales.Review, Permissions.Sales.Cancel, Permissions.Sales.Complete,
        Permissions.Sales.Void, Permissions.Sales.PrintReceipt, Permissions.Sales.ViewTransactions,

        Permissions.Prescriptions.View, Permissions.Prescriptions.Create,
        Permissions.Prescriptions.Dispense, Permissions.Prescriptions.Approve,
        Permissions.Prescriptions.ReviewControlled, Permissions.Prescriptions.Override,

        Permissions.Patients.View, Permissions.Patients.ViewBasic, Permissions.Patients.Create,
        Permissions.Patients.Edit, Permissions.Patients.ViewHistory,

        Permissions.Inventory.View, Permissions.Inventory.ViewStockOnly, Permissions.Inventory.AddStock,
        Permissions.Inventory.EditStock, Permissions.Inventory.AdjustStock,
        Permissions.Inventory.ApproveAdjustment, Permissions.Inventory.StockCount,

        Permissions.Billing.View, Permissions.Billing.GenerateInvoice, Permissions.Billing.ReceivePayment,
        Permissions.Billing.CompleteSale, Permissions.Billing.PrintReceipt,
        Permissions.Billing.Refund, Permissions.Billing.Void,

        Permissions.Reports.ViewAll, Permissions.Reports.ViewFinancial, Permissions.Reports.ViewOperational,
        Permissions.Reports.ViewInventory, Permissions.Reports.ViewPrescription, Permissions.Reports.ViewPersonalSales,

        Permissions.Suppliers.View, Permissions.Suppliers.Create, Permissions.Suppliers.Edit,
        Permissions.Suppliers.ManageOrders,

        Permissions.Settings.View, Permissions.Settings.ManageUsers, Permissions.Settings.ManageRoles,
        Permissions.Settings.ManageSystem, Permissions.Settings.ManageDrugCatalog,
    ];

    private static readonly string[] ManagerPermissions =
    [
        Permissions.Sales.View, Permissions.Sales.Complete, Permissions.Sales.Void,
        Permissions.Sales.ViewTransactions,

        Permissions.Prescriptions.View,

        Permissions.Patients.View,

        Permissions.Inventory.View,

        Permissions.Billing.View, Permissions.Billing.Refund, Permissions.Billing.Void,

        Permissions.Reports.ViewAll, Permissions.Reports.ViewFinancial, Permissions.Reports.ViewOperational,

        Permissions.Suppliers.View, Permissions.Suppliers.Create, Permissions.Suppliers.Edit,
        Permissions.Suppliers.ManageOrders,
    ];

    private static readonly string[] ChiefPharmacistPermissions =
    [
        Permissions.Sales.View, Permissions.Sales.Create, Permissions.Sales.AddItems,
        Permissions.Sales.Review, Permissions.Sales.Cancel,

        Permissions.Prescriptions.View, Permissions.Prescriptions.Create,
        Permissions.Prescriptions.Dispense, Permissions.Prescriptions.Approve,
        Permissions.Prescriptions.ReviewControlled, Permissions.Prescriptions.Override,

        Permissions.Patients.View, Permissions.Patients.Create, Permissions.Patients.Edit,
        Permissions.Patients.ViewHistory,

        Permissions.Inventory.View, Permissions.Inventory.AddStock, Permissions.Inventory.EditStock,
        Permissions.Inventory.AdjustStock, Permissions.Inventory.ApproveAdjustment,
        Permissions.Inventory.StockCount,

        Permissions.Billing.View,

        Permissions.Reports.ViewAll, Permissions.Reports.ViewInventory, Permissions.Reports.ViewPrescription,

        Permissions.Suppliers.View, Permissions.Suppliers.Create, Permissions.Suppliers.Edit,
        Permissions.Suppliers.ManageOrders,

        Permissions.Settings.View, Permissions.Settings.ManageDrugCatalog,
    ];

    private static readonly string[] PharmacistPermissions =
    [
        Permissions.Sales.View, Permissions.Sales.Create, Permissions.Sales.AddItems,
        Permissions.Sales.Review, Permissions.Sales.Cancel,

        Permissions.Prescriptions.View, Permissions.Prescriptions.Create,
        Permissions.Prescriptions.Dispense,

        Permissions.Patients.View, Permissions.Patients.Create, Permissions.Patients.Edit,
        Permissions.Patients.ViewHistory,

        Permissions.Inventory.View, Permissions.Inventory.StockCount,

        Permissions.Billing.View,

        Permissions.Reports.ViewInventory, Permissions.Reports.ViewPrescription,

        Permissions.Suppliers.View,
    ];

    private static readonly string[] CashierPermissions =
    [
        Permissions.Sales.View, Permissions.Sales.AddItems, Permissions.Sales.Complete,
        Permissions.Sales.PrintReceipt,

        Permissions.Patients.ViewBasic,

        Permissions.Inventory.ViewStockOnly,

        Permissions.Billing.View, Permissions.Billing.GenerateInvoice, Permissions.Billing.ReceivePayment,
        Permissions.Billing.CompleteSale, Permissions.Billing.PrintReceipt,

        Permissions.Reports.ViewPersonalSales,
    ];

    private static readonly Dictionary<string, string[]> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [Roles.Admin] = AdminPermissions,
        [Roles.Manager] = ManagerPermissions,
        [Roles.ChiefPharmacist] = ChiefPharmacistPermissions,
        [Roles.Pharmacist] = PharmacistPermissions,
        [Roles.Cashier] = CashierPermissions,
    };

    public static string[] GetPermissions(string role) =>
        Map.TryGetValue(role, out var perms) ? perms : [];
}
