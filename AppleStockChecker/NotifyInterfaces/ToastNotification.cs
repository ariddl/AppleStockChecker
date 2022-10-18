using AppleStockChecker.Notifier;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Text;

namespace AppleStockChecker.NotifyInterfaces
{
    internal class ToastNotification : INotifyInterface
    {
        private const int MaxMessagesPerToast = 4;

        private bool _notifyOnError;

        public bool Init(Configuration config)
            => (_notifyOnError = config.notifyOnError) || true;

        public async Task Notify(Notification notification)
        {
            for (int i = 0; i < notification.Available.Count; i += MaxMessagesPerToast)
            {
                var sb = new StringBuilder();
                for (int j = i; j < Math.Min(i + MaxMessagesPerToast, notification.Available.Count); ++j)
                    sb.AppendLine(notification.Available[j].Message);

                var builder = new ToastContentBuilder()
                   .AddText($"Stock Update - {notification.StoreInfo}")
                   .AddText(sb.ToString());
                builder.Show();
            }
            await Task.CompletedTask;
        }

        public async Task NotifyError(string title, string message)
        {
            if (!_notifyOnError)
                return;
            new ToastContentBuilder()
                .AddText($"Error: {title}")
                .AddText(message)
                .Show();
            await Task.CompletedTask;
        }
    }
}