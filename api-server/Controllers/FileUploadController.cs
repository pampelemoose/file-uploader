using api_server.Models;
using api_server.Protocol;
using Csv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Xml;
using static api_server.Protocol.FileUploadDto;

namespace api_server.Controllers;

[ApiController]
[Route("[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IDbContextFactory<UploadDbContext> _dbContext;

    public FileUploadController(IDbContextFactory<UploadDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] FileUploadDto.Request req
    )
    {
        using var context = await _dbContext.CreateDbContextAsync();

        var query = context.Transactions.AsNoTracking();

        if (req.Currency is not null)
            query = query.Where(_ => _.CurrencyCode == req.Currency);

        if (req.From is not null)
            query = query.Where(_ => _.TransactedAt >= req.From);

        if (req.To is not null)
            query = query.Where(_ => _.TransactedAt <= req.To);

        if (req.Status is not null)
            query = query.Where(_ => _.Status == req.Status);

        var transactions = query.Select(_ => new FileUploadDto.Response
        {
            Id = _.TransactionId, 
            Payment = $"{_.Amount.ToString("0.00")} {_.CurrencyCode}",
            Status = _.Status.ToExternal()
        }).ToList();

        return StatusCode((int)HttpStatusCode.OK, transactions);
    }


    [HttpPost()]
    public async Task<IActionResult> PostUploadFile(
        IFormFile file
    )
    {
        try
        {
            var allowedTypes = new List<string> { "text/xml", "text/csv" };

            if (!allowedTypes.Contains(file.ContentType))
                return StatusCode((int)HttpStatusCode.PreconditionFailed, "Unknown format");

            return file.ContentType switch
            {
                "text/xml" => await _parseXml(file),
                "text/csv" => await _parseCsv(file),
                _ => StatusCode((int)HttpStatusCode.BadRequest, "Unknown format")
            };
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private async Task<IActionResult> _parseXml(IFormFile file)
    {
        using var context = await _dbContext.CreateDbContextAsync();

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var document = new XmlDocument();
        document.LoadXml(content);

        if (document.ChildNodes.Count != 2)
            return StatusCode((int)HttpStatusCode.PreconditionFailed);

        var transactionsRaw = document.SelectSingleNode("Transactions");

        if (transactionsRaw is null)
            return StatusCode((int)HttpStatusCode.PreconditionFailed);

        foreach (XmlNode line in transactionsRaw.ChildNodes)
        {
            string transactionId = line.Attributes?.GetNamedItem("id")?.Value ?? string.Empty;

            var paymentDetails = line.SelectSingleNode("PaymentDetails");
            if (paymentDetails is null)
                return StatusCode((int)HttpStatusCode.PreconditionFailed);

            string accountNo = paymentDetails.SelectSingleNode("AccountNo")?.InnerText ?? string.Empty;
            decimal amount = decimal.Parse(paymentDetails.SelectSingleNode("Amount")?.InnerText ?? "-1");
            string currencyCode = paymentDetails.SelectSingleNode("CurrencyCode")?.InnerText ?? string.Empty;

            var transactionDate = line.SelectSingleNode("TransactionDate");
            if (transactionDate is null)
                return StatusCode((int)HttpStatusCode.PreconditionFailed);

            DateTime transactedAt = DateTime.Parse(transactionDate.InnerText);

            var statusRaw = line.SelectSingleNode("Status");
            if (statusRaw is null)
                return StatusCode((int)HttpStatusCode.PreconditionFailed);

            TransactionStatus status = Enum.Parse<TransactionStatus>(statusRaw.InnerText);

            if (string.IsNullOrEmpty(transactionId)
                || string.IsNullOrEmpty(accountNo)
                || string.IsNullOrEmpty(currencyCode)
                || amount < 0)
                return StatusCode((int)HttpStatusCode.InternalServerError);

            context.Transactions.Add(new Transaction
            {
                TransactionId = transactionId,
                AccountNo = accountNo,
                CurrencyCode = currencyCode,
                Amount = amount,
                TransactedAt = transactedAt,
                Status = status,
            });
        }

        if (await context.SaveChangesAsync() <= 0)
            return StatusCode((int)HttpStatusCode.InternalServerError);

        return StatusCode((int)HttpStatusCode.OK);
    }

    private async Task<IActionResult> _parseCsv(IFormFile file)
    {
        using var context = await _dbContext.CreateDbContextAsync();

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var options = new CsvOptions { 
            HeaderMode = HeaderMode.HeaderAbsent,
            TrimData = true,
        };

        foreach (var line in CsvReader.ReadFromText(content, options))
        {
            if (line.ColumnCount != 6)
                return StatusCode((int)HttpStatusCode.InternalServerError);

            string transactionId = line.Values[0];
            string accountNo = line.Values[1];
            decimal amount = decimal.Parse(line.Values[2]);
            string currencyCode = line.Values[3];
            
            DateTime transactedAt = DateTime.ParseExact(line.Values[4], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            TransactionStatus status = Enum.Parse<TransactionStatus>(_swapStatusCsvToXml(line.Values[5]));

            if (string.IsNullOrEmpty(transactionId)
                || string.IsNullOrEmpty(accountNo)
                || string.IsNullOrEmpty(currencyCode)
                || amount < 0)
                return StatusCode((int)HttpStatusCode.InternalServerError);

            context.Transactions.Add(new Transaction
            {
                TransactionId = transactionId,
                AccountNo = accountNo,
                CurrencyCode = currencyCode,
                Amount = amount,
                TransactedAt = transactedAt,
                Status = status,
            });
        }

        if (await context.SaveChangesAsync() <= 0)
            return StatusCode((int)HttpStatusCode.InternalServerError);

        return StatusCode((int)HttpStatusCode.OK);
    }

    private string _swapStatusCsvToXml(string status)
    {
        return status switch
        {
            "Approved" => "Approved",
            "Failed" => "Rejected",
            "Finished" => "Done",
            _ => ""
        };
    }
}
