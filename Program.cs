using System.Text;
using Impostor_Router;
using static Impostor_Router.TokenController;

// 设置控制台输出为 UTF-8
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// 加载配置文件
builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

// 配置日志记录
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 从config.json获取端口配置
var port = builder.Configuration.GetValue<int>("port");
var forwardingOptions = new ForwardingOptions
{
    Forward1 = builder.Configuration.GetValue<int>("forward1"),
    Forward2 = builder.Configuration.GetValue<int>("forward2"),
    Forward3 = builder.Configuration.GetValue<int>("forward3")
};

// 使用ILogger记录配置值
var logger = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
}).CreateLogger("Startup");

logger.LogInformation($"主监听端口: {port}");
logger.LogInformation($"转发端口1 对应618: {forwardingOptions.Forward1}");
logger.LogInformation($"转发端口2 对应64: {forwardingOptions.Forward2}");
logger.LogInformation($"转发端口3 对应354: {forwardingOptions.Forward3}");

// 注册ForwardingOptions到服务容器
builder.Services.AddSingleton(forwardingOptions);

// 设置监听端口
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(port); // 使用配置文件中的端口
});

// 注册控制器和HttpClient
builder.Services.AddControllers();
builder.Services.AddHttpClient<UserController>();
builder.Services.AddHttpClient<GamesController>();

var app = builder.Build();

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class ForwardingOptions
{
    public int Forward1 { get; set; }
    public int Forward2 { get; set; }
    public int Forward3 { get; set; }
}
