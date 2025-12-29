using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace LifeOS.API.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("LifeOSPostgreConnectionString") 
            ?? throw new InvalidOperationException("Connection string 'LifeOSPostgreConnectionString' not found.");
        
        // Seq URL yapılandırması - Docker ve Local ortam desteği
        // Öncelik sırası: Environment Variable > appsettings.json > Default
        var seqUrl = Environment.GetEnvironmentVariable("Serilog__SeqUrl")
            ?? builder.Configuration["Serilog:SeqUrl"]
            ?? (builder.Environment.IsDevelopment() ? "http://localhost:5341" : null);
        
        var seqApiKey = Environment.GetEnvironmentVariable("Serilog__SeqApiKey")
            ?? builder.Configuration["Serilog:SeqApiKey"];
        
        var environment = builder.Environment.EnvironmentName;

        // PostgreSQL için tablo yapılandırması
        IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "message", new RenderedMessageColumnWriter() },
            { "message_template", new MessageTemplateColumnWriter() },
            { "level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter() },
            { "exception", new ExceptionColumnWriter() },
            { "properties", new LogEventSerializedColumnWriter() },
            { "props_test", new PropertiesColumnWriter() },
            { "machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.Raw) }
        };

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)

            // Enrichers - Context bilgilerini ekle
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "LifeOS")
            .Enrich.WithProperty("Environment", environment)

            // Console sink - Development için
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
            )

            // File sink - Her gün yeni dosya, 31 gün saklama
            .WriteTo.File(
                path: "logs/baseproject-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                fileSizeLimitBytes: 10 * 1024 * 1024 // 10 MB
            )

            // PostgreSQL sink - Structured logging için
            // ⚠️ Production'da sadece Warning ve üzeri loglar veritabanına kaydedilmeli (performans için)
            // Development'ta Information seviyesi kabul edilebilir
            .WriteTo.PostgreSQL(
                connectionString: connectionString,
                tableName: "Logs",
                columnOptions: columnWriters,
                needAutoCreateTable: true,
                restrictedToMinimumLevel: environment == "Development" ? LogEventLevel.Information : LogEventLevel.Warning
            );

        // Seq sink - Development/Production log analizi için
        // Seq URL null ise Seq sink'i ekleme (opsiyonel)
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Seq(
                serverUrl: seqUrl,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey
            );
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog();
    }
}
