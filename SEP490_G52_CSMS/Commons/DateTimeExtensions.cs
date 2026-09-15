namespace SEP490_G52_CSMS.Commons
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                }
                catch
                {
                    return TimeZoneInfo.CreateCustomTimeZone("Vietnam Time", TimeSpan.FromHours(7), "Vietnam Time", "Vietnam Time");
                }
            }
        }

        public static DateTime ToVietnamTime(this DateTime dt)
        {
            var utcTime = dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
        }

        public static DateTime? ToVietnamTime(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return dt.Value.ToVietnamTime();
        }

        public static string ToVietnamTimeString(this DateTime dt, string format = "dd/MM/yyyy HH:mm")
        {
            return dt.ToVietnamTime().ToString(format);
        }

        public static string ToVietnamTimeString(this DateTime? dt, string format = "dd/MM/yyyy HH:mm")
        {
            return dt.HasValue ? dt.Value.ToVietnamTimeString(format) : "";
        }
    }
}
