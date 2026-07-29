using Cike.Locks.DistributedRedis;
using Medallion.Threading.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Cike.Locks.Distributed;

[DependsOn([typeof(CikeLocksModule)])]
public class CikeLockDistributedRedisModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IDistributedLockProvider>(serviceProvider =>
        {
            var redisDistributedLockOptions = serviceProvider.GetService<IOptionsMonitor<CikeRedisDistributedLockOptions>>();
            if (redisDistributedLockOptions==null
                ||redisDistributedLockOptions.CurrentValue==null
                || (redisDistributedLockOptions.CurrentValue.RedisConnectionString.IsNullOrEmpty() && redisDistributedLockOptions.CurrentValue.RedisDatabase == null))
            {
                throw new ApplicationException("Please use Config<CikeRedisDistributedLockOptions>");
            }
            var database = redisDistributedLockOptions.CurrentValue.RedisDatabase==null? ConnectionMultiplexer.Connect(redisDistributedLockOptions.CurrentValue.RedisConnectionString!).GetDatabase(): redisDistributedLockOptions.CurrentValue.RedisDatabase;
            return new RedisDistributedSynchronizationProvider(database);
        });
        await base.ConfigureServicesAsync(context);
    }
}
