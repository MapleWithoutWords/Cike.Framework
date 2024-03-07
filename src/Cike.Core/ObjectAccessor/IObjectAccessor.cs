namespace Cike.Core.ObjectAccessor;

public interface IObjectAccessor<out T>
{
    T? Value { get; }
}
