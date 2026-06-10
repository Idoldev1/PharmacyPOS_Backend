namespace POS.API.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Cashier = "Cashier";
        public const string Pharmacist = "Pharmacist";

        public static readonly string[] All =
        {
            Admin,
            Manager,
            Cashier,
            Pharmacist
        };

        public static string? Normalize(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            return All.FirstOrDefault(r =>
                string.Equals(r, role.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsValid(string? role) => Normalize(role) is not null;
    }
}