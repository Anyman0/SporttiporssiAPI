namespace SporttiporssiAPI.Helpers
{
    public class TimestampConverter
    {
        public static long ConvertToTimestamp(DateTime dateTime)
        {
            // Assuming the timestamp is in UTC and is in the format of YYYYMMDDHHmmss
            return long.Parse(dateTime.ToString("yyyyMMddHHmmss"));
        }

        public static DateTime ConvertToDateTime(long timestamp)
        {
            var timestampString = timestamp.ToString();

            // Check if the timestamp is at least 14 digits (YYYYMMDDHHMMSS)
            if (timestampString.Length != 14)
            {
                throw new ArgumentException("Invalid timestamp format. Expected format: YYYYMMDDHHMMSS");
            }

            var year = int.Parse(timestampString.Substring(0, 4));
            var month = int.Parse(timestampString.Substring(4, 2));
            var day = int.Parse(timestampString.Substring(6, 2));
            var hour = int.Parse(timestampString.Substring(8, 2));
            var minute = int.Parse(timestampString.Substring(10, 2));
            var second = int.Parse(timestampString.Substring(12, 2));

            var parsedDate = new DateTime(year, month, day, hour, minute, second);

            // Ensure the parsed date is within the valid SQL datetime range
            if (parsedDate < new DateTime(1753, 1, 1) || parsedDate > new DateTime(9999, 12, 31))
            {
                throw new ArgumentOutOfRangeException("Timestamp is out of SQL Server's valid datetime range.");
            }

            return parsedDate;
        }

    }
}
