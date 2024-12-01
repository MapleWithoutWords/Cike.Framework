namespace Cike.Caching.DistributedCache.Internal.Enumerations;

internal enum CompressMode
{
    /// <summary>
    /// no compression
    /// </summary>
    None = 1,

    /// <summary>
    /// Compress but not deserialize
    /// </summary>
    Compress,

    /// <summary>
    /// serialize and compress
    /// </summary>
    SerializeAndCompress,
}
