
namespace Gabbro_Secret_Manager.Core
{
    public class ToastableException(ToastSeverity severity, Exception inner) : Exception
    {
        public ToastSeverity Severity { get; init; } = severity;
        public Exception Inner { get; init; } = inner;
    }

    public enum ToastSeverity
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
