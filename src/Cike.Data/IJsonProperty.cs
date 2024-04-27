
namespace Cike.Data;

public interface IJsonEntity<T>
{
    T JsonProperty { get; set; }
}
