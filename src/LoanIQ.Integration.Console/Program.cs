using FluentValidation;
using LoanIQ.Integration.BLL.Audit;
using LoanIQ.Integration.BLL.Configuration;
using LoanIQ.Integration.BLL.Mapping;
using LoanIQ.Integration.BLL.Repositories;
using LoanIQ.Integration.BLL.Services;
using LoanIQ.Integration.BLL.Validation;
using LoanIQ.Integration.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration))
        .ConfigureServices((context, services) =>
        {
            services.Configure<ConsoleSettings>(
                context.Configuration.GetSection(ConsoleSettings.SectionName));

            services.Configure<GlExtractSettings>(
                context.Configuration.GetSection(GlExtractSettings.SectionName));

            services.Configure<PaymentExtractSettings>(
                context.Configuration.GetSection(PaymentExtractSettings.SectionName));

            services.AddAutoMapper(typeof(CustomerProfile).Assembly);

            services.AddValidatorsFromAssemblyContaining<CustomerFileRecordValidator>();

            var auditConnectionString = context.Configuration[$"{AuditSettings.SectionName}:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(auditConnectionString))
            {
                services.Configure<AuditSettings>(
                    context.Configuration.GetSection(AuditSettings.SectionName));
                services.AddSingleton<IAuditEventPublisher, AuditEventPublisher>();
            }
            else
            {
                services.AddSingleton<IAuditEventPublisher, NullAuditEventPublisher>();
            }

            services.AddSingleton<IMisCodeRepository, MisCodeRepository>();
            services.AddSingleton<CustomerFileProcessor>();
            services.AddHostedService<CustomerFileWatcherService>();

            services.AddSingleton<GlExtractService>();
            services.AddHostedService<GlExtractJob>();

            services.AddSingleton<PaymentExtractService>();
            services.AddHostedService<PaymentExtractJob>();
        })
        .Build();

    await host.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Console application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
