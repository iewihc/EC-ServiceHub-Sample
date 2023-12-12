using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "Order";
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
    )
    ;
// AddConsoleExporter());


builder.Services.AddHttpClient();

var app = builder.Build();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseSwagger();
app.UseSwaggerUI();

var activitySource = new ActivitySource(serviceName);

app.MapPost("/create-order", async (HttpRequest request, HttpClient httpClient) =>
{
    // 開始一個新的活動（Activity）用於跟踪創建訂單的過程
    using var mainActivity = activitySource.StartActivity("Create Order");

    // 為主活動添加一些自定義標籤
    mainActivity?.SetTag("order.id", "特123456");
    mainActivity?.SetTag("order.amount", 99.99);
    mainActivity?.SetTag("customer.id", "cust789");

    // 假設這裡是訂單的詳細信息
    var orderDetails = new {Items = new[] {("Item1", 10), ("Item2", 20)}};

    string orderNumber;
    using (var generateOrderNumberActivity = activitySource.StartActivity("Generate Order Number"))
    {
        // 生成訂單編號的邏輯
        orderNumber = Guid.NewGuid().ToString();
    }

    double total;
    using (var calculateTotalActivity = activitySource.StartActivity("Calculate Total"))
    {
        // 計算總價的邏輯
        total = orderDetails.Items.Sum(item => item.Item2);
    }
    // 準備向支付服務發送請求
    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost:50052/process-payment");
    requestMessage.Content = new StringContent($"Order Number: {orderNumber}, Total: {total}");

    // 發送請求並等待響應
    var response = await httpClient.SendAsync(requestMessage);

    // 返回創建訂單成功的信息
    return Results.Ok($"創建訂單成功. 訂單編號: {orderNumber}, 總價: {total}. 支付服務回應: {response.StatusCode}");
});


app.Run();