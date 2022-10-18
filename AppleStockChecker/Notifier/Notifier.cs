using AppleStockChecker.NotifyInterfaces;
using System.Reflection;

namespace AppleStockChecker.Notifier
{
    internal class Notifier
    {
        private Configuration _config;
        private Dictionary<string, string> _models;
        private NotifierProfile[] _profiles;
        private SemaphoreSlim _querySemaphore;
        private List<INotifyInterface> _notificationInterfaces;

        public Notifier(Configuration config)
        {
            _config = config;
            _models = new Dictionary<string, string>();
            _profiles = new NotifierProfile[config.profiles.Length];
            _querySemaphore = new SemaphoreSlim(1, 1);
            _notificationInterfaces = new List<INotifyInterface>();

            foreach (Configuration.Model model in config.models)
                _models.TryAdd(model.part, model.name);

            for (int i = 0; i < _profiles.Length; ++i)
                _profiles[i] = new NotifierProfile(this, config, i);

            AddNotificationType(new ConsoleNotification());
            AddNotificationType(new ToastNotification());
            LoadNotifyModules();
        }

        private void AddNotificationType(INotifyInterface @interface)
        {
            if (@interface.Init(_config))
                _notificationInterfaces.Add(@interface);
        }

        private void LoadNotifyModules()
        {
            foreach (string module in _config.notifyModules)
            {
                string fileName = $"{module}.dll";
                if (!File.Exists(fileName))
                    continue;

                try
                {
                    Assembly asm = Assembly.LoadFrom(fileName);
                    foreach (Type t in asm.GetTypes())
                    {
                        if (!t.GetInterfaces().Contains(typeof(INotifyInterface)))
                            continue;
                        var @interface = Activator.CreateInstance(t) as INotifyInterface;
                        if (@interface == null)
                            continue;
                        AddNotificationType(@interface);
                        Console.WriteLine($"Using notification type {t.Name} from {asm.GetName().Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load notify module assembly {module}: {ex.Message}");
                }
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await Delay(_config.initialWaitMs, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
                await Task.WhenAll(_profiles.Select(profile => profile.Run(cancellationToken)));
        }

        public string LookupModel(string partNo)
            => _models.TryGetValue(partNo, out var model) ? model : "<UNKNOWN>";

        internal async Task BeginQuery(CancellationToken cancellationToken)
            => await _querySemaphore.WaitAsync(cancellationToken);

        internal void EndQuery()
            => _querySemaphore.Release();

        // ContinueWith needed to avoid TaskCanceledException upon graceful exit
        internal static async Task Delay(int ms, CancellationToken cancellationToken)
            => await Task.Delay(ms, cancellationToken).ContinueWith(_ => { });

        internal async Task Notify(Notification notification)
            => await Task.WhenAll(_notificationInterfaces.Select(notify => notify.Notify(notification)).ToArray());

        internal async Task NotifyError(string title, string message)
            => await Task.WhenAll(_notificationInterfaces.Select(notify => notify.NotifyError(title, message)).ToArray());
    }
}