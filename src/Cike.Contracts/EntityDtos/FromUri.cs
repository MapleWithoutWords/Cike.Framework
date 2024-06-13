using Cike.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace Cike.Contracts.EntityDtos;

public abstract class FromUri<T> where T : class, new()
{
    public static ValueTask<T?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var result = new T();

        foreach (var propertyItem in typeof(T).GetProperties())
        {
            if (propertyItem.PropertyType == typeof(List<DynamicQuery>))
            {
                propertyItem.SetValue(result, GetDynamicQueryDto(context.Request.Query));
            }
            else
            {
                if (context.Request.Query.TryGetValue(propertyItem.Name, out StringValues value))
                {
                    propertyItem.SetValue(result, Convert.ChangeType(value, propertyItem.PropertyType));
                }
                else if (context.Request.Query.TryGetValue(propertyItem.Name.FistCharToLower(), out value))
                {
                    propertyItem.SetValue(result, Convert.ChangeType(value, propertyItem.PropertyType));
                }
            }
        }

        return ValueTask.FromResult<T?>(result);
    }


    public static List<DynamicQuery> GetDynamicQueryDto(IQueryCollection queries)
    {
        List<DynamicQuery> list = new List<DynamicQuery>();
        foreach (var item in queries)
        {
            if (item.Value.Count < 1)
            {
                continue;
            }
            if (string.IsNullOrEmpty(item.Value[0]))
            {
                continue;
            }
            DynamicQuery param = new DynamicQuery();
            param.Key = item.Key.Trim().FistCharToUpper();
            param.Value = item.Value.Where(e => e.IsNullOrEmpty() == false).FirstOrDefault();
            list.Add(param);
        }

        return list;
    }
}