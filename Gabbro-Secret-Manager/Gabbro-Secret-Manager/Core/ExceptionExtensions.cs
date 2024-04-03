namespace Gabbro_Secret_Manager.Core
{
    public static class ExceptionExtensions
    {
        public static ToastableException Toast(this Exception exception, ToastSeverity severity) => new ToastableException(severity, exception);
    }
}
