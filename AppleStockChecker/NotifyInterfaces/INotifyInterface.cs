using AppleStockChecker.Notifier;

namespace AppleStockChecker.NotifyInterfaces
{
    public interface INotifyInterface
    {
        public bool Init(Configuration config) => true;
        public Task Notify(Notification notification);
        public Task NotifyError(string title, string message);
    }
}