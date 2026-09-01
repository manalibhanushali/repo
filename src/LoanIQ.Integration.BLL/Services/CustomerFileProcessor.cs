using System.Text.Json;
using AutoMapper;
using FluentValidation;
using LoanIQ.Integration.BLL.Audit;
using LoanIQ.Integration.DTO;
using LoanIQ.Integration.BLL.Repositories;
using Microsoft.Extensions.Logging;

namespace LoanIQ.Integration.BLL.Services;

public sealed class CustomerFileProcessor(
    IMapper mapper,
    IValidator<CustomerFileRecord> validator,
    IAuditEventPublisher auditPublisher,
    IMisCodeRepository misCodeRepository,
    ILogger<CustomerFileProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ProcessingResult> ProcessAsync(string filePath, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing file {FilePath}", filePath);

        string json;
        try
        {
            json = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read file {FilePath}", filePath);
            return ProcessingResult.Failure($"Failed to read file: {ex.Message}");
        }

        CustomerFileRecord? record;
        try
        {
            record = JsonSerializer.Deserialize<CustomerFileRecord>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "File {FilePath} contains invalid JSON", filePath);
            return ProcessingResult.Failure($"Invalid JSON: {ex.Message}");
        }

        if (record is null)
        {
            logger.LogError("File {FilePath} deserialised to null", filePath);
            return ProcessingResult.Failure("File content is null or empty.");
        }

        var validationResult = await validator.ValidateAsync(record, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            foreach (var error in errors)
                logger.LogWarning("Validation failed for {FilePath}: {Error}", filePath, error);

            return ProcessingResult.Failure(errors);
        }

        try
        {
            var misCode = await misCodeRepository.GetMisCodeAsync(record.CompanyId, cancellationToken);
            record = record with { MisCode = misCode };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MISCode lookup failed for CompanyId {CompanyId}; fallback concatenation will be used", record.CompanyId);
        }

        CustomerLiqRequest mapped;
        try
        {
            mapped = mapper.Map<CustomerLiqRequest>(record);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Mapping failed for file {FilePath}", filePath);
            return ProcessingResult.Failure($"Mapping error: {ex.Message}");
        }

        logger.LogInformation(
            "Customer {OperationType} processed: externalId={ExternalId}, name={Name}",
            mapped.OperationType, mapped.ExternalId, mapped.FullName);

        try
        {
            await auditPublisher.PublishAsync(new CustomerAuditEvent
            {
                OperationType = mapped.OperationType,
                ExternalId = mapped.ExternalId,
                FileName = Path.GetFileName(filePath),
                PublishedAt = DateTimeOffset.UtcNow,
                Success = true,
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Audit event publishing failed for {FilePath}; processing continues", filePath);
        }

        return ProcessingResult.Success();
    }
}
