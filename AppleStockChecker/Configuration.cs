namespace AppleStockChecker
{
    public sealed class Configuration
    {
        public sealed class Model
        {
            public string name = "";
            public string part = "";
            public bool active = true;
        }

        public sealed class Profile
        {
            public string name = "";
            public int postalCode = 12180;
            public string preferredStore = "";
            public double maxStoreDistance = 0;
            public int checkMins = 1;
            public int suppressInStockMins = 5;
            public bool active = true;
        }

        public string fulfillmentApi = "https://www.apple.com/shop/fulfillment-messages";
        
        public Dictionary<string, string> fixedParams = new Dictionary<string, string>()
        {
            { "pl", "true" },
            { "cppart", "UNLOCKED/US" },
        };

        public Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36" },
            { "cache-control", "no-cache" },
            { "pragma", "no-cache" },
            { "referer", "https://www.apple.com/shop/buy-iphone/iphone-14-pro/6.7-inch-display-128gb-deep-purple-unlocked" },
            { "sec-ch-ua", "\"Chromium\"; v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"" }
        };

        public Model[] models = new Model[0];

        public Profile[] profiles = new Profile[0];

        public string[] notifyModules = new string[0];

        public int initialWaitMs = 5000;

        public int queryTimeoutMs = 60000;

        public int retryWaitMs = 250;

        public int retryAttempts = 3;

        public bool notifyOnError = true;
    }
}