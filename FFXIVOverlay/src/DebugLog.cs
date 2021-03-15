using ff14bot.Helpers;
using System.Globalization;

namespace FFXIVOverlay
{
    public static class DebugLog
    {
        private static System.Windows.Media.Color infoColor = System.Windows.Media.Colors.DarkKhaki;

        public static void Info(string message)
        {
            Logging.Write(infoColor, message);
        }

        //[StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            Logging.Write(infoColor, string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}