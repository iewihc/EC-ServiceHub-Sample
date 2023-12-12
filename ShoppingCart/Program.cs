using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "ShoppingCart";
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
        // .AddConsoleExporter()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        })
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        // .AddConsoleExporter()
        .AddMeter("ShoppingCartMetrics")
        .AddPrometheusExporter()
    );


builder.Services.AddHttpClient();

var app = builder.Build();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseSwagger();
app.UseSwaggerUI();

var activitySource = new ActivitySource(serviceName);
var orderServiceUrl = builder.Configuration["Services:OrderSrv"];

var meter = new Meter("ShoppingCartMetrics", "1.0");
var addToCartCounter = meter.CreateCounter<int>("add-to-cart");

app.MapPost("/add-to-cart/{memberId}", async (int memberId, HttpRequest request, ILogger<Program> logger, HttpClient httpClient) =>
{
    using var activity = activitySource.StartActivity("Add to Cart");
    addToCartCounter.Add(1);
    // 添加商品到購物車的邏輯...
    logger.LogWarning("執行添加商品到購物車的邏輯...");

    // 向訂單服務(Order Service) 發送請求
    var requestMessage = new HttpRequestMessage(HttpMethod.Post, orderServiceUrl + "/create-order");

    var response = await httpClient.SendAsync(requestMessage);
    return Results.Ok($"商品添加到會員 {memberId} 的購物車. 訂單服務回應: {response.StatusCode}");
});

app.Run();