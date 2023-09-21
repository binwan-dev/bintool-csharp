namespace System;

public static class CheckExtension
{
    public static T NotNull<T>(this T val, string tipsMessage)
    {
        if (val == null)
        {
            throw new ArgumentNullException(tipsMessage);
        }

        var strVal = val as string;
        if (strVal != null && strVal == "")
        {
            throw new ArgumentNullException(tipsMessage);
        }

        return val;
    }

    public static T[] CheckLength<T>(this T[] tlist, int min, int max, string tipsMessage)
    {
        if (tlist.Length < min || tlist.Length > max)
        {
            throw new ArgumentException(tipsMessage);
        }

        return tlist;
    }

    public static string IPv4(this string val, string tipsMessage)
    {
        var regex = @"(\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5])";
        if (!Regex.IsMatch(val, regex))
        {
            throw new ArgumentException(tipsMessage);
        }

        return val;
    }

    public static int Port<T>(this T val, string tipsMessage)
    {
        int? intVal;

        var strVal = val as string;
        if (strVal != null && int.TryParse(strVal, out int intVals))
        {
            intVal = intVals;
        }
        else
        {
            intVal = val as int?;
        }

        if (intVal == null || intVal <= 0 || intVal > 65535)
        {
            throw new ArgumentException(tipsMessage);
        }

        return intVal.Value;
    }

    public static bool MustTrue(this bool val, string tipsMessage)
    {
	if(!val)
	{
	    throw new ArgumentException(tipsMessage);
	}

	return val;
    }
}
