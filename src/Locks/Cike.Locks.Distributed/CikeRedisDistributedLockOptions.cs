using Cike.Locks.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Locks.DistributedRedis;

public class CikeRedisDistributedLockOptions
{
    public string? RedisConnectionString { get; set; }

    public IDatabase? RedisDatabase { get; set; }
}
