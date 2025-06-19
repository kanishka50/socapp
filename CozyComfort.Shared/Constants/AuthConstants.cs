namespace CozyComfort.Shared.Constants
{
    public static class AuthConstants
    {
        public const string JwtSecretKey = "Jwt:Key";
        public const string JwtIssuerKey = "Jwt:Issuer";
        public const string JwtAudienceKey = "Jwt:Audience";
        public const string JwtExpiryInMinutesKey = "Jwt:ExpiryInMinutes";

        public const string DefaultPassword = "Password123!";

        public static class Policies
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string RequireManufacturerRole = "RequireManufacturerRole";
            public const string RequireDistributorRole = "RequireDistributorRole";
            public const string RequireSellerRole = "RequireSellerRole";
        }
    }
}