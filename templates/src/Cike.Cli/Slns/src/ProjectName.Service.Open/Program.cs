var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/logs.log", outputTemplate: "{Timestamp:HH:mm} || {Level} || {SourceContext:l} || {Message} || {Exception} ||end {NewLine}", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.AddSerilog();

await builder.Services.AddApplicationAsync<ProjectNameServiceOpenModule>();
var app = builder.Build();

await app.InitializeApplicationAsync();
app.MapGet("/", () => "Hello Project!");

app.Run();
