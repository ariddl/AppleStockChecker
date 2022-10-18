using AppleStockChecker;
using AppleStockChecker.Notifier;
using AppleStockChecker.NotifyInterfaces;
using Newtonsoft.Json;
using System.Text;
using Twilio.Rest.Api.V2010.Account;

namespace Twilio
{
    public class TwilioSmsNotification : INotifyInterface
    {
        private class TwilioConfiguration
        {
            public string accountSid = "";
            public string authToken = "";
            public string fromNumber = "";
            public string toNumber = "";
        }

        private TwilioConfiguration? _config;
        private bool _notifyOnError;

        public bool Init(Configuration config)
        {
            const string twilioCfg = "twilio.json";
            if (!File.Exists(twilioCfg))
            {
                File.WriteAllText(twilioCfg, JsonConvert.SerializeObject(new TwilioConfiguration(), Formatting.Indented));
                Console.WriteLine($"{GetType().Name}: Complete config and restart");
                return false;
            }

            TwilioConfiguration? twilioConfig = JsonConvert.DeserializeObject<TwilioConfiguration>(File.ReadAllText(twilioCfg));
            if (twilioConfig == null)
            {
                Console.WriteLine($"{GetType().Name}: Failed to load config");
                return false;
            }

            _config = twilioConfig;
            _notifyOnError = config.notifyOnError;
            TwilioClient.Init(twilioConfig.accountSid, twilioConfig.authToken);
            return true;
        }

        public async Task Notify(Notification notification)
        {
            if (notification.Available.Count == 0)
                return;
            var sb = new StringBuilder();
            sb.AppendLine($"Stock Update - {notification.StoreInfo}");
            foreach (Notification.Availability availability in notification.Available)
                sb.AppendLine(availability.Message);
            await SendMessage(sb.ToString());
        }

        public async Task NotifyError(string title, string message)
            => await (_notifyOnError ? SendMessage($"Error - {title}: {message}") : Task.CompletedTask);

        private async Task SendMessage(string body)
            => await MessageResource.CreateAsync(body: body, from: _config?.fromNumber, to: _config?.toNumber);
    }
}