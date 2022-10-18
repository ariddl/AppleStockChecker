using AppleStockChecker;
using AppleStockChecker.Notifier;
using Newtonsoft.Json;

const string cfg = "config.json";

var config = new Configuration();

if (!File.Exists(cfg))
{
    File.WriteAllText(cfg, JsonConvert.SerializeObject(config, Formatting.Indented));
    Console.WriteLine("Complete config and start again");
    return;
}

config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(cfg));
if (config == null)
{
    Console.WriteLine("Failed to load configuration");
    return;
}

var notif = new Notifier(config);
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;
Task task = notif.Run(cancellationToken);

Console.WriteLine("AppleStockChecker is now active. Q to quit.");
while (Console.ReadKey().Key != ConsoleKey.Q)
    continue;

cancellationTokenSource.Cancel();
await task;