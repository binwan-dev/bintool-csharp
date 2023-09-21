namespace System;

public static class DateTimeExtension
{
    public static long ToTimestamp(this DateTime dateTime, TimeZoneInfo? zone = null)
    {
        zone = zone ?? TimeZoneInfo.Utc;
        var startTime = new System.DateTime(1970, 1, 1) + (zone.BaseUtcOffset);
        return (long)(dateTime - startTime).TotalSeconds;
    }
}
