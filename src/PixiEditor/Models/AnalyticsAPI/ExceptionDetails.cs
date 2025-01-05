using PixiEditor.Helpers;

namespace PixiEditor.Models.AnalyticsAPI;

public class ExceptionDetails
{
    /// <summary>
    /// The fully qualified type name of the exception.
    /// </summary>
    public string ExceptionType { get; set; }
    
    /// <summary>
    /// The exception message.
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// The exception stack trace.
    /// </summary>
    public string StackTrace { get; set; }
    
    /// <summary>
    /// A collection of <see cref="ExceptionDetails"/> that represent inner or aggregated exceptions.
    /// </summary>
    public List<ExceptionDetails> InnerExceptions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionDetails"/> class
    /// from the specified <see cref="Exception"/>.
    /// </summary>
    /// <param name="ex">The exception to capture details from.</param>
    public ExceptionDetails(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        ExceptionType = ex.GetType().FullName;
        Message = CrashHelper.TrimFilePaths(ex.Message);
        StackTrace = ex.StackTrace;
        InnerExceptions = [];

        if (ex is AggregateException aggEx)
        {
            foreach (var innerException in aggEx.InnerExceptions)
            {
                InnerExceptions.Add(new ExceptionDetails(innerException));
            }
        }
        else if (ex.InnerException != null)
        {
            InnerExceptions.Add(new ExceptionDetails(ex.InnerException));
        }
    }
}
