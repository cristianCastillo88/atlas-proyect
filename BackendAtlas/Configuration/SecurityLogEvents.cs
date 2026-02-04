using Microsoft.Extensions.Logging;

namespace BackendAtlas.Configuration
{
    public static class SecurityLogEvents
    {
        public static readonly EventId LoginSuccess = new(1001, "LoginSuccess");
        public static readonly EventId LoginFailed = new(1002, "LoginFailed");
        
        public static readonly EventId PasswordChangeSuccess = new(2001, "PasswordChangeSuccess");
        public static readonly EventId PasswordChangeFailed = new(2002, "PasswordChangeFailed");
        
        public static readonly EventId PasswordResetRequested = new(3001, "PasswordResetRequested");
        public static readonly EventId PasswordResetSuccess = new(3002, "PasswordResetSuccess");
        public static readonly EventId PasswordResetFailed = new(3003, "PasswordResetFailed");
        
        public static readonly EventId AdminPasswordReset = new(4001, "AdminPasswordReset");
    }
}
