using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace RobotCoinV3PublishRedis
{
    internal class TelegramBot
    {
        string TOKEN_BOT;
        string CHATID_ERROR { get; }
        string CHATID_STATUS { get; }
        string CHATID_INFO { get; }

        public TelegramBot(string _TOKEN_BOT, string _CHATID_ERROR, string _CHATID_STATUS, string _CHATID_INFO)
        {
            TOKEN_BOT = _TOKEN_BOT;
            CHATID_ERROR = _CHATID_ERROR;
            CHATID_STATUS = _CHATID_STATUS;
            CHATID_INFO = _CHATID_INFO;
        }

        public async Task SendErrorAsync(string Msg)
        {
            var bot = new TelegramBotClient(TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(CHATID_ERROR, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Telegram Exception) " + ex.Message);
            }
        }

        public async Task SendMessageAsync(string Msg)
        {
            var bot = new TelegramBotClient(TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(CHATID_INFO, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Telegram Exception) " + ex.Message);
            }
        }

        public async Task SendStatusAsync(string Msg)
        {
            var bot = new TelegramBotClient(TOKEN_BOT);
            try
            {
                await bot.SendTextMessageAsync(CHATID_STATUS, Msg[..Math.Min(Msg.Length, 4095)], ParseMode.Html);
            }
            catch (Exception ex)
            {
                throw new Exception("Undefined Error (Send Telegram Exception) " + ex.Message);
            }
        }
    }
}
