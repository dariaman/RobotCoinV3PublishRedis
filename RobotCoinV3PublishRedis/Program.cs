using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RobotCoinV3PublishRedis;
using StackExchange.Redis;

var setttings = new Settings();

var ListCoinNotif = setttings.ListCoinNotif;

DateTime DATE_NOW = DateTime.Now;
if (setttings.TELEGRAM_TOKEN_BOT == null || setttings.TELEGRAM_CHATID_ERROR == null || setttings.TELEGRAM_CHATID_STATUS == null || setttings.TELEGRAM_CHATID_INFO == null)
{
    Console.WriteLine(DATE_NOW.ToString("yyyyMMddHHmmss") + "\nTelegram Key is null");
    Environment.Exit(0);
}

TelegramBot _telegram = new(setttings.TELEGRAM_TOKEN_BOT, setttings.TELEGRAM_CHATID_ERROR, setttings.TELEGRAM_CHATID_STATUS, setttings.TELEGRAM_CHATID_INFO);

if (setttings.INDODAX_PRICE_URL == null)
{
    await _telegram.SendErrorAsync(DATE_NOW.ToString("yyyyMMddHHmmss") + "\nIndodaxPriceUrl is null");
    Environment.Exit(0);
}

if (setttings.NICEHASH_PRICE_URL == null)
{
    await _telegram.SendErrorAsync(DATE_NOW.ToString("yyyyMMddHHmmss") + "\nNicehash Env is null");
    Environment.Exit(0);
}

await _telegram.SendStatusAsync("Tgl Start Pubs =>" + DATE_NOW.ToString("dd MMM yyyy HH:mm:ss") + $" >> " + DATE_NOW.ToString("yyyyMMddHHmmss"));

using ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(setttings.REDIS_CON_STRING);
ISubscriber sub = redis.GetSubscriber();

var listHarga = await GetCoinPriceAsync();
if (listHarga?.Count > 0) await PublishCoinAsync(listHarga, "coin");

//Console.ReadKey();

async Task<List<CoinPrice>?> GetCoinPriceAsync()
{
    try
    {
        var coinSetting = new Settings();

        HttpClient client = new();
        // Ambil Price dari nicehash
        var resp = await client.GetAsync(coinSetting.NICEHASH_PRICE_URL).Result.Content.ReadAsStringAsync();
        var _data_price2 = JsonConvert.DeserializeObject(resp);

        // Ambil Price dari Indodax
        resp = await client.GetAsync(coinSetting.INDODAX_PRICE_URL).Result.Content.ReadAsStringAsync();
        JObject jObject = JObject.Parse(resp);
        if (jObject["tickers"] == null) return null;

        var listCoinPrice = new List<CoinPrice>();
        foreach (var item in coinSetting.FavoriteCoinList)
        {
            CoinPrice coinPrice = new()
            {
                CoinCode = item.ToUpper(),
                DateString = DATE_NOW.ToString("yyyyMMddHHmmss")
            };

            // ambil nilai USDT dari data nicehash
            decimal? _temp;
            if (item.ToUpper() == "INCH")
                _temp = (decimal?)((JProperty)((JContainer)_data_price2).Where(x => x.Path == "ONEINCHUSDT").FirstOrDefault())?.Value;
            else
                _temp = (decimal?)((JProperty)((JContainer)_data_price2).Where(x => x.Path == item.ToUpper() + "USDT").FirstOrDefault())?.Value;

            if (_temp != null) coinPrice.USDT = _temp ?? 0;

            // ambil nilai BTC dari data nicehash
            if (item.ToUpper() == "INCH")
                _temp = (decimal?)((JProperty)((JContainer)_data_price2).Where(x => x.Path == "ONEINCHBTC").FirstOrDefault())?.Value;
            else
                _temp = (decimal?)((JProperty)((JContainer)_data_price2).Where(x => x.Path == item.ToUpper() + "BTC").FirstOrDefault())?.Value;

            if (_temp != null) coinPrice.BTC = _temp ?? 0;

            // ambil nilai IDR dari data indodax
            int? _temp2;
            if (item.ToUpper() == "INCH")
                _temp2 = (int?)jObject["tickers"]?[$"1{item.ToLower()}_idr"]?["last"];
            else
                _temp2 = (int?)jObject["tickers"]?[$"{item.ToLower()}_idr"]?["last"];

            coinPrice.IDR = _temp2 ?? 0;

            if (coinPrice.BTC > 0 || coinPrice.IDR > 0 || coinPrice.USDT > 0) listCoinPrice.Add(coinPrice);
        }

        return listCoinPrice;
    }
    catch { throw; }
}

async Task PublishCoinAsync(List<CoinPrice> listCoinPrice, string channel)
{
    try
    {
        foreach (CoinPrice xcoinPrice in listCoinPrice) sub.Publish(channel, JsonConvert.SerializeObject(xcoinPrice));
    }
    catch (Exception ex)
    {
        await _telegram.SendErrorAsync("Tgl =>" + DATE_NOW.ToString("dd MMM yyyy HH:mm:ss") + "\n" + (ex.InnerException?.Message ?? ex.Message));
    }
}