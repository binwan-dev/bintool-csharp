namespace System;

public static class CheckExtension
{
    public static T NotNull<T>(this T val, string tipsMessage, Dictionary<string,string>? data=null)
    {
        if (val == null) throw new CheckException(tipsMessage);

        var strVal = val as string;
        if (strVal != null && strVal == "")
        {
            if (data != null) tipsMessage = SetTipsData(tipsMessage, data);  
            throw new CheckException(tipsMessage);
        }

        return val;
    }

    public static T NotNull<T>(this T val, CheckExceptionInfo tips, Dictionary<string,string>? data=null)
    {
	if(val==null)throw new CheckException(tips);
        return val;
    }

    public static string NotNullOrWhiteSpace(this string val, string tipsMessage, Dictionary<string,string>? data=null)
    {
	if(string.IsNullOrWhiteSpace(val)) throw new CheckException(tipsMessage);
        return val;
    }

    public static string NotNullOrWhiteSpace(this string val, CheckExceptionInfo tipsMessage, Dictionary<string,string>? data=null)
    {
	if(string.IsNullOrWhiteSpace(val)) throw new CheckException(tipsMessage);
        return val;
    }

    public static bool MustBeTrue(this bool val, string tipsMessage, Dictionary<string,string>? data=null)
    {
        if (!val) throw new CheckException(tipsMessage);
        return val;
    }

    public static bool MustBeTrue(this bool val, CheckExceptionInfo tipsMessage, Dictionary<string,string>? data=null)
    {
        if (!val) throw new CheckException(tipsMessage);
        return val;
    }

    public static bool MustBeFalse(this bool val, string tipsMessage, Dictionary<string,string>? data=null)
    {
        if (val) throw new CheckException(tipsMessage);
        return val;
    }

    public static bool MustBeFalse(this bool val, CheckExceptionInfo tipsMessage, Dictionary<string,string>? data=null)
    {
        if (val) throw new CheckException(tipsMessage);
        return val;
    }

    public static T[] CheckLength<T>(this T[] tlist, int min, int max, string tipsMessage, Dictionary<string,string>? data=null)
    {
        if (tlist.Length < min || tlist.Length > max)
        {
            throw new ArgumentException(tipsMessage);
        }

        return tlist;
    }

    public static string IPv4(this string val, string tipsMessage, Dictionary<string,string>? data=null)
    {
        var regex = @"(\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5])";
        if (!Regex.IsMatch(val, regex))
        {
            throw new ArgumentException(tipsMessage);
        }

        return val;
    }

    public static int Port<T>(this T val, string tipsMessage, Dictionary<string,string>? data=null)
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

    public static bool MustTrue(this bool val, string tipsMessage, Dictionary<string, string>? data = null)
    {
        if (!val)
        {
            throw new ArgumentException(tipsMessage);
        }

        return val;
    }

    private static string SetTipsData(string tips,Dictionary<string,string> data)
    {
        foreach (var item in data)
        {
            tips = tips.Replace(item.Key, item.Value);
        }

        return tips;
    }
}

public class CheckExceptionInfo
{
    public CheckExceptionInfo(string message) : this(999, message)
    { }

    public CheckExceptionInfo(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public int Code{ get; set; }

    public string Message { get; set; } = string.Empty;
}

public class CheckException : Exception
{
    public CheckException(string message) : base(message)
    {
        Info = new CheckExceptionInfo(message);
    }

    public CheckException(CheckExceptionInfo info) : base(info.Message)
    {
        Info = info;
    }

    public CheckExceptionInfo Info{ get; set; }
}
