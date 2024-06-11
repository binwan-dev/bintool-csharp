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

    public static string ToStr(this byte[]? payload) => ToStr(payload, Encoding.UTF8);

    public static short ToShort(this byte[]? payload,int startIndex=0,int count=0)
    {
        if(payload==null||payload.Length==0)
        {
            return 0;
        }

        if(count == 0)
        {
            count=payload.Length;
        }

        var buffer= payload.Skip(startIndex).Take(count).ToArray();
        return BitConverter.ToInt16(buffer, 0);
    }

    public static int ToInteger(this string strVal,int defaultVal=0)
    {
        if(string.IsNullOrWhiteSpace(strVal)||!int.TryParse(strVal,out int val))
        {
            return defaultVal;
        }
        return val;
    }

    public static long ToLong(this string strVal,long defaultVal=0)
    {
        if(string.IsNullOrWhiteSpace(strVal)||!long.TryParse(strVal,out long val))
        {
            return defaultVal;
        }
        return val;
    }

    public static double ToDouble(this string strVal,double defaultVal=0)
    {
        if(string.IsNullOrWhiteSpace(strVal)||!double.TryParse(strVal,out double val))
        {
            return defaultVal;
        }
        return val;
    }
}
