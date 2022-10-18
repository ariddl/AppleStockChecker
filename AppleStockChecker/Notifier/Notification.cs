using AppleStockChecker.Apple;

namespace AppleStockChecker.Notifier
{
    public sealed class Notification
    {
        public sealed class Availability
        {
            public PartAvailability Part;
            public string Model = "";
            public string Message = "";
        }

        public Store StoreInfo;
        public List<Availability> Available = new List<Availability>();
    }
}