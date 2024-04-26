using Snowflake.Core;

namespace System;

public static class SnowflakeExtension
{
    private static IdWorker Worker = new IdWorker(1, 1);

    public static void SetWorker(int workerId, int dataCenterId, int sequence = 0)
    {
        Worker = new IdWorker(workerId, dataCenterId, sequence);
    }

    public static long NewId()
    {
        return Worker.NextId();
    }
}
