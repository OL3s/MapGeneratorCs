namespace MapGeneratorCs.Logging;
public class TimeLogger
{
    public DateTime StartTime { get; set; } = DateTime.Now;
    private bool includePrintLog = false;
    public TimeLogger(bool includePrintLog = true)
    {
        this.includePrintLog = includePrintLog;
        StartTime = DateTime.Now;
    }
    public void Print(string message, bool printTime = true)
    {
        if (includePrintLog == false)
            return;

        var endTime = DateTime.Now;
        var duration = endTime - StartTime;
        string logMessage = printTime
            ? $"{message} in {duration.TotalMilliseconds} ms"
            : message;
        Console.WriteLine(logMessage);
        StartTime = DateTime.Now;
    }
}