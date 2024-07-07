using System.Text;
using Impostor_Router;
using static Impostor_Router.TokenController;

// ���ÿ���̨���Ϊ UTF-8
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// ���������ļ�
builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

// ������־��¼
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ��config.json��ȡ�˿�����
var port = builder.Configuration.GetValue<int>("port");
var forwardingOptions = new ForwardingOptions
{
    Forward1 = builder.Configuration.GetValue<int>("forward1"),
    Forward2 = builder.Configuration.GetValue<int>("forward2"),
    Forward3 = builder.Configuration.GetValue<int>("forward3")
};

// ʹ��ILogger��¼����ֵ
var logger = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
}).CreateLogger("Startup");

logger.LogInformation($"�������˿�: {port}");
logger.LogInformation($"ת���˿�1 ��Ӧ618: {forwardingOptions.Forward1}");
logger.LogInformation($"ת���˿�2 ��Ӧ64: {forwardingOptions.Forward2}");
logger.LogInformation($"ת���˿�3 ��Ӧ354: {forwardingOptions.Forward3}");

// ע��ForwardingOptions����������
builder.Services.AddSingleton(forwardingOptions);

// ���ü����˿�
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(port); // ʹ�������ļ��еĶ˿�
});

// ע���������HttpClient
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
