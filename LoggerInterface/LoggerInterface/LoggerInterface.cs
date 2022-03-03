namespace LoggerTemplate
{
    public interface LoggerInterface
    {
        void LogError(string message);
        void LogWarning(string message);
        void LogInfo(string message);
        void LogException(Exception ex);

    }
}