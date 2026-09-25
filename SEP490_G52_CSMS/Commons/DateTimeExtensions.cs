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

        /// <summary>
        /// Đảm bảo thời gian theo giờ Việt Nam. Nếu là UTC thì chuyển sang GMT+7; nếu đã là giờ Local/Unspecified (được lưu theo giờ VN) thì giữ nguyên.
        /// </summary>
        public static DateTime EnsureVietnamTime(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dt, VietnamTimeZone);
            }
            return dt;
        }

        public static DateTime? EnsureVietnamTime(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return dt.Value.EnsureVietnamTime();
        }

        public static string EnsureVietnamTimeString(this DateTime dt, string format = "dd/MM/yyyy HH:mm")
        {
            return dt.EnsureVietnamTime().ToString(format);
        }

        public static string EnsureVietnamTimeString(this DateTime? dt, string format = "dd/MM/yyyy HH:mm")
        {
            return dt.HasValue ? dt.Value.EnsureVietnamTimeString(format) : "";
        }

        /// <summary>
        /// Chuyển đổi mốc thời gian (UTC) thành chuỗi hiển thị khoảng thời gian tương đối (relative time).
        /// Hỗ trợ cả thời điểm quá khứ (vừa xong, phút trước, giờ trước, hôm qua, ngày trước...)
        /// và thời điểm tương lai / hạn chót (hôm nay, ngày mai, trong X ngày, quá hạn X ngày...).
        /// </summary>
        public static string ToRelativeTimeString(this DateTime dt, DateTime? referenceUtc = null, bool isDateOnly = false)
        {
            var refTime = referenceUtc.HasValue ? referenceUtc.Value.ToVietnamTime() : DateTime.UtcNow.ToVietnamTime();
            var targetTime = dt.ToVietnamTime();

            if (isDateOnly)
            {
                var targetDate = targetTime.Date;
                var refDate = refTime.Date;
                var diffDays = (targetDate - refDate).Days;

                if (diffDays == 0) return "Hôm nay";
                if (diffDays == 1) return "Ngày mai";
                if (diffDays > 1 && diffDays <= 30) return $"Trong {diffDays} ngày";
                if (diffDays > 30) return $"Trong {Math.Max(1, (int)Math.Round(diffDays / 30.0))} tháng";
                if (diffDays == -1) return "Hôm qua (Quá hạn 1 ngày)";
                return $"Quá hạn {Math.Abs(diffDays)} ngày";
            }

            var delta = refTime - targetTime;

            // Past times (delta >= 0)
            if (delta.TotalSeconds >= 0)
            {
                if (delta.TotalSeconds < 60) return "Vừa xong";
                if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes} phút trước";
                if (delta.TotalHours < 24 && targetTime.Date == refTime.Date) return $"{(int)delta.TotalHours} giờ trước";
                if ((refTime.Date - targetTime.Date).Days == 1) return "Hôm qua";
                if (delta.TotalDays < 30) return $"{Math.Max(1, (int)delta.TotalDays)} ngày trước";
                if (delta.TotalDays < 365) return $"{Math.Max(1, (int)(delta.TotalDays / 30))} tháng trước";
                return $"{Math.Max(1, (int)(delta.TotalDays / 365))} năm trước";
            }
            else // Future times (delta < 0)
            {
                var futureDelta = targetTime - refTime;
                if (targetTime.Date == refTime.Date) return "Hôm nay";
                if ((targetTime.Date - refTime.Date).Days == 1) return "Ngày mai";
                if (futureDelta.TotalDays < 30) return $"Trong {Math.Max(1, (int)futureDelta.TotalDays)} ngày";
                if (futureDelta.TotalDays < 365) return $"Trong {Math.Max(1, (int)(futureDelta.TotalDays / 30))} tháng";
                return $"Trong {Math.Max(1, (int)(futureDelta.TotalDays / 365))} năm";
            }
        }

        public static string ToRelativeTimeString(this DateTime? dt, DateTime? referenceUtc = null, bool isDateOnly = false)
        {
            return dt.HasValue ? dt.Value.ToRelativeTimeString(referenceUtc, isDateOnly) : "";
        }

        /// <summary>
        /// Alias cho ToRelativeTimeString để đồng bộ tên gọi formatRelativeTime giữa backend và client.
        /// Cho phép truyền mốc thời gian referenceUtc (optional, mặc định là hiện tại).
        /// </summary>
        public static string FormatRelativeTime(this DateTime dt, DateTime? referenceUtc = null, bool isDateOnly = false)
        {
            return dt.ToRelativeTimeString(referenceUtc, isDateOnly);
        }

        public static string FormatRelativeTime(this DateTime? dt, DateTime? referenceUtc = null, bool isDateOnly = false)
        {
            return dt.HasValue ? dt.Value.ToRelativeTimeString(referenceUtc, isDateOnly) : "";
        }
    }
}
