namespace api.Extensions;

public class EnvironmentVariables
{
    public static string JwtKey => GetRequired("JWT_KEY");
    public static string GetRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Environment variable '{key}' is missing.");
        }

        return value;
    }
    public static string GoogleClientId => GetRequired("GOOGLE_CLIENT_ID");
    public static string GoogleClientSecret => GetRequired("GOOGLE_CLIENT_SECRET");
    public static string PayPalClientId => GetRequired("PAYPAL_CLIENT_ID");
    public static string PayPalClientSecret => GetRequired("PAYPAL_CLIENT_SECRET");
    public static string SmtpSender => GetRequired("SMTP_MAIL_SENDER");
    public static string SmtpUsername => GetRequired("SMTP_USERNAME");
    public static string SmtpPassword => GetRequired("SMTP_PASSWORD");
    public static string InitialAdminPassword => GetRequired("INITIAL_ADMIN_PASSWORD");
    public static string ConnectionString => GetRequired("MSSQL_CONNECTION_STRING");
}