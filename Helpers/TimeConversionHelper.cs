namespace SporttiporssiAPI.Helpers
{
    public class TimeConversionHelper
    {
        public static string ConvertSecondsToMinutesAndSeconds(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}.{seconds}";
        }
    }
}
