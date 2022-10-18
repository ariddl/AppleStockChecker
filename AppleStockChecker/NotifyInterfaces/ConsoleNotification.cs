using AppleStockChecker.Notifier;

namespace AppleStockChecker.NotifyInterfaces
{
    internal class ConsoleNotification : INotifyInterface
    {
        public async Task Notify(Notification notification)
        {
            Console.Write($"[{GetTimestamp()}] {notification.StoreInfo}: ");

            if (notification.Available.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Out of stock");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("STOCK AVAILABLE!");
            foreach (Notification.Availability availability in notification.Available)
                Console.WriteLine($"- {availability.Message}");
            Console.ResetColor();
            await Task.CompletedTask;
        }

        public async Task NotifyError(string title, string message)
        {
            Console.WriteLine($"[{GetTimestamp()}] Error: {title} ({message})");
            await Task.CompletedTask;
        }

        private static string GetTimestamp()
            => DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
    }
}