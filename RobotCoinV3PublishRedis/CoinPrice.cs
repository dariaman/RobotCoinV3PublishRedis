namespace RobotCoinV3PublishRedis
{
    internal class CoinPrice
    {
        public string? CoinCode { get; set; }
        public string? DateString { get; set; }
        public decimal USDT { get; set; } = 0;
        public decimal BTC { get; set; } = 0;
        public int IDR { get; set; } = 0;
    }
}
