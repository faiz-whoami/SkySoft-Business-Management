namespace business_managment_system.Helpers
{
    public static class TransactionNames
    {
        public const string Sale = "Sale";
        public const string Purchase = "Purchase";
        public const string Service = "Service";

        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static bool IsKnownType(string name)
        {
            return string.Equals(name, Sale, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, Purchase, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, Service, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
