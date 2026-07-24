namespace POS.API.Constants;

public static class Permissions
{
    public static class Sales
    {
        public const string View = "sales.view";
        public const string Create = "sales.create";
        public const string AddItems = "sales.add_items";
        public const string Review = "sales.review";
        public const string Cancel = "sales.cancel";
        public const string Complete = "sales.complete";
        public const string Void = "sales.void";
        public const string PrintReceipt = "sales.print_receipt";
        public const string ViewTransactions = "sales.view_transactions";
    }

    public static class Prescriptions
    {
        public const string View = "prescriptions.view";
        public const string Create = "prescriptions.create";
        public const string Dispense = "prescriptions.dispense";
        public const string Approve = "prescriptions.approve";
        public const string ReviewControlled = "prescriptions.review_controlled";
        public const string Override = "prescriptions.override";
    }

    public static class Patients
    {
        public const string View = "patients.view";
        public const string ViewBasic = "patients.view_basic";
        public const string Create = "patients.create";
        public const string Edit = "patients.edit";
        public const string ViewHistory = "patients.view_history";
    }

    public static class Inventory
    {
        public const string View = "inventory.view";
        public const string ViewStockOnly = "inventory.view_stock_only";
        public const string AddStock = "inventory.add_stock";
        public const string EditStock = "inventory.edit_stock";
        public const string AdjustStock = "inventory.adjust_stock";
        public const string ApproveAdjustment = "inventory.approve_adjustment";
        public const string StockCount = "inventory.stock_count";
    }

    public static class Billing
    {
        public const string View = "billing.view";
        public const string GenerateInvoice = "billing.generate_invoice";
        public const string ReceivePayment = "billing.receive_payment";
        public const string CompleteSale = "billing.complete_sale";
        public const string PrintReceipt = "billing.print_receipt";
        public const string Refund = "billing.refund";
        public const string Void = "billing.void";
    }

    public static class Reports
    {
        public const string ViewAll = "reports.view_all";
        public const string ViewFinancial = "reports.view_financial";
        public const string ViewOperational = "reports.view_operational";
        public const string ViewInventory = "reports.view_inventory";
        public const string ViewPrescription = "reports.view_prescription";
        public const string ViewPersonalSales = "reports.view_personal_sales";
    }

    public static class Suppliers
    {
        public const string View = "suppliers.view";
        public const string Create = "suppliers.create";
        public const string Edit = "suppliers.edit";
        public const string ManageOrders = "suppliers.manage_orders";
    }

    public static class Settings
    {
        public const string View = "settings.view";
        public const string ManageUsers = "settings.manage_users";
        public const string ManageRoles = "settings.manage_roles";
        public const string ManageSystem = "settings.manage_system";
        public const string ManageDrugCatalog = "settings.manage_drug_catalog";
    }
}
