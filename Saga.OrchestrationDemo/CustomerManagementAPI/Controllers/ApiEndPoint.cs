namespace CustomerManagementAPI.Controllers
{
    public class ApiEndPoint
    {
        public const string Register = "Register";
        public const string UndoRegister = "UndoRegister/{emailAddress}";
        public const string SendWelcomeEmail = "SendWelcomeEmail/{emailAddress}";
    }
}