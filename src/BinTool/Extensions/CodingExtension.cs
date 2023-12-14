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

    public static byte[] ToUtf8Bytes(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return Array.Empty<byte>();
        }

        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// string to bytes
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding">default: utf-8</param>
    /// <returns></returns>
    public static byte[] ToBytes(this string str, Encoding? encoding = null)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return Array.Empty<byte>();
        }

        encoding ??= Encoding.UTF8;
        return encoding.GetBytes(str);
    }
}
