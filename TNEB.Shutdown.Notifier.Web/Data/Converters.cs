using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TNEB.Shutdown.Notifier.Web.Data
{
    public static class Converters
    {
        public static ValueConverter<DateTimeOffset, long> DateTimeOffsetToUnix = new ValueConverter<DateTimeOffset, long>(
               dateTimeOffset => dateTimeOffset.ToUnixTimeMilliseconds(),
               unixTime => DateTimeOffset.FromUnixTimeMilliseconds(unixTime).ToLocalTime());
    }
}
