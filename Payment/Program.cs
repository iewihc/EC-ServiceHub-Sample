using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "Payment";
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName));
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost"; // 或 Jaeger 運行的主機名稱
            options.AgentPort = 6831; // 根據您的設置可能需要更改
        })
    )
    .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter()
        // .AddConsoleExporter()
    );


builder.Services.AddHttpClient();

var app = builder.Build();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseSwagger();
app.UseSwaggerUI();

var activitySource = new ActivitySource(serviceName);

app.MapPost("/process-payment", (HttpRequest request) =>
{
    using var activity = activitySource.StartActivity("Process Payment");
    return Results.Ok("支付處理成功");
});

app.Run();