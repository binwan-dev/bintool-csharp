namespace System;

public static class ExceptionExtension
{
    public static TException? As<TException>(this Exception ex) where TException:Exception
    {
        if (ex is TException exception)
        {
            return exception;
        }

        if (ex.InnerException != null)
        {
            return As<TException>(ex.InnerException);
        }

        return default;
    }
}