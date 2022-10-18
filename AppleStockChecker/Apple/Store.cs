namespace AppleStockChecker.Apple
{
    public struct Store
    {
        public string storeName;
        public string state;
        public string country;
        public string city;
        public string storeNumber;
        public double storedistance;
        public string storeDistanceWithUnit;

        public override string ToString() 
            => $"{storeName} ({storeDistanceWithUnit})";
    }
}