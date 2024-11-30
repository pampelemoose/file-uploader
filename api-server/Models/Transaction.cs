using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_server.Models;

[PrimaryKey(nameof(Id))]
public class Transaction
{
    public Guid Id { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(50)]
    public string TransactionId { get; set; } = string.Empty;
    [Column(TypeName = "VARCHAR")]
    [StringLength(30)]
    public string AccountNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TransactedAt { get; set; }
    public TransactionStatus Status { get; set; }

}
