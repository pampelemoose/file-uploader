using api_server.Models;

namespace api_server.Protocol;

public static class FileUploadDto
{
    public record Request
    {
        public string? Currency { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public TransactionStatus? Status { get; set; }
    }

    public record Response
    {
        public string Id { get; set; } = string.Empty;
        public string Payment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
