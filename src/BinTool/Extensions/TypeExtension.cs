namespace System;

public static class TypeExtension
{
    public static byte[] ToBytes(this string payload, Encoding? coding = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return new byte[0];
        }
        coding = coding ?? Encoding.UTF8;

        return coding.GetBytes(payload);
    }

    public static string ToStr(this byte[]? payload, Encoding? coding = null)
    {
	if (payload==null || payload.Length == 0)
	{
            return string.Empty;
        }
        coding = coding ?? Encoding.UTF8;

        return coding.GetString(payload);
    }
}
