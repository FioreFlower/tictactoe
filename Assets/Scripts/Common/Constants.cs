public class Constants
{
    public const string ServerURL = "http://localhost:3000";

    public enum SigninResultType
    {
        UserNameNotFound,
        IncorrectPassword,
        Success,
    }
}