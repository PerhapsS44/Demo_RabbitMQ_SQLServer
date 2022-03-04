using LoggerTemplate;
using System.Text;

namespace Logger
{
    public sealed class Logger : LoggerInterface
    {
        private static Logger instance = null;
        private static readonly object padlock = new object();

        private FileStream logFile;
        private string fileName = null;
        private Logger()
        {
        }
        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }
        public void InitLogger(string args)
        {
            if (this.fileName != null)
            {
                logFile.Close();
            }
            this.fileName = args;

            logFile = new FileStream(instance.fileName, FileMode.Append);
        }

        public void LogError(string message)
        {
            AppendTextToFile($"[error] {message}");
            Console.WriteLine($"[error] {message}");
        }

        public void LogException(Exception ex)
        {
            AppendTextToFile($"[exception] {ex.Message}");
        }

        public void LogInfo(string message)
        {
            AppendTextToFile($"[info] {message}");
        }

        public void LogWarning(string message)
        {
            AppendTextToFile($"[warning] {message}");
        }

        private void AppendTextToFile(string message)
        {
            ReadOnlySpan<byte> toBeWritten = new ReadOnlySpan<byte>(Encoding.ASCII.GetBytes(message + "\n"));
            logFile.Write(toBeWritten);
            logFile.Flush();
        }
    }
}
