namespace Cike.UniversalId.ULong;

public class SnowflakeIdGenerator : ISnowflakeIdGenerator
{
    private const int MachineIdBits = 10; // 机器ID所占的位数
    private const int SequenceBits = 12; // 序列在ID中占的位数

    private const long MaxMachineId = -1L ^ (-1L << MachineIdBits); // 支持的最大机器id，结果是1023
    private const long MaxSequence = -1L ^ (-1L << SequenceBits); // 同一毫秒内序列的最大值，结果是4095

    private const int TimestampLeftShift = SequenceBits + MachineIdBits; // 时间戳向左移12+10=22位
    private const int MachineIdShift = SequenceBits; // 机器ID向左移12位

    private long machineId; // 机器ID
    private long sequence = 0L; // 序列号
    private long lastTimestamp = -1L; // 上次生成ID的时间戳

    private static object _lock = new object();

    public SnowflakeIdGenerator(long machineId)
    {
        if (machineId > MaxMachineId || machineId < 0)
        {
            throw new ArgumentException($"MachineId must be between 0 and {MaxMachineId}");
        }
        this.machineId = machineId;
    }

    public long NextId()
    {
        lock (_lock)
        {
            long timestamp = GetTimestamp();

            if (timestamp < lastTimestamp)
            {
                throw new Exception("Clock moved backwards. Refusing to generate id for " + (lastTimestamp - timestamp) + " milliseconds");
            }

            if (lastTimestamp == timestamp)
            {
                sequence = (sequence + 1) & MaxSequence;
                if (sequence == 0)
                {
                    timestamp = WaitNextMillis(lastTimestamp);
                }
            }
            else
            {
                sequence = 0;
            }

            lastTimestamp = timestamp;
            return ((timestamp - Twepoch) << TimestampLeftShift) | (machineId << MachineIdShift) | sequence;
        }
    }

    private long WaitNextMillis(long lastTimestamp)
    {
        long timestamp = GetTimestamp();
        while (timestamp <= lastTimestamp)
        {
            timestamp = GetTimestamp();
        }
        return timestamp;
    }

    private long GetTimestamp()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    private const long Twepoch = 1288834974657L; // Twitter的时间戳修正值
}
