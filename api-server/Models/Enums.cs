namespace api_server.Models;

public enum TransactionStatus
{
    Approved,
    Rejected,
    Done
}

public static class StatusExtension
{
    public static string ToExternal(this TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.Approved => "A",
            TransactionStatus.Rejected => "R",
            TransactionStatus.Done => "D",
            _ => ""
        };
    }
}