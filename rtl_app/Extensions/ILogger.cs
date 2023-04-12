public static class ILoggerExtensions
{
    public static void Log<T>(this ILogger<T> logger, string message, Func<bool> predicate, System.ConsoleColor color)
    {
        //logic to format time to the right
        for (int i = message.Length; i <= 40; i++)
        {
            message += " ";
        }
        message += $"- {DateTime.Now.ToString("HH:mm:ss")}";
        if(predicate() == false) return;
        switch(color){
            case ConsoleColor.Red:
                logger.LogError(message);
                break;
            case ConsoleColor.Yellow:
                logger.LogWarning(message);
                break;
            default:
                logger.LogInformation(message);
                break;            
        }
    }
}