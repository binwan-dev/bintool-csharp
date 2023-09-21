namespace System;

public static class CodingExtension
{
    public static string DecodeByUnicode(this string unicode)
    {
        if (string.IsNullOrWhiteSpace(unicode))
        {
            return string.Empty;
        }

        return Regex.Unescape(unicode);
    }

    public static string EncodeToUtf8(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str));
    }
}
