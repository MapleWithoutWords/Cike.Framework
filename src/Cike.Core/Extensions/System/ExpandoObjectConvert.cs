using System.Collections;
using System.Linq.Expressions;

namespace Cike.Core.Extensions.System;

public static class DictionaryConvert
{
    public static Dictionary<string, object> Convert<TSource>(TSource source)
    {
        var sourceType = source!.GetType();
        if (!ExpressionCahce<TSource>.Converts.ContainsKey(sourceType.FullName!))
        {
            ExpressionCahce<TSource>.Converts[sourceType.FullName!] = ToDictionaryExpression<TSource>().Compile();
        }

        return ExpressionCahce<TSource>.Converts[sourceType.FullName!].Invoke(source);
    }

    public static Expression<Func<T, Dictionary<string, object>>> ToDictionaryExpression<T>()
    {
        var param = Expression.Parameter(typeof(T), "p");
        var dictionary = Expression.Variable(typeof(Dictionary<string, object>), "dict");

        var dictionaryAssign = Expression.Assign(dictionary, Expression.New(typeof(Dictionary<string, object>)));

        var body = new List<Expression> { dictionaryAssign };

        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            var propValue = Expression.Property(param, prop);
            Expression value;
            if (prop.PropertyType.IsGenericType && prop.PropertyType.IsAssignableTo(typeof(ICollection)) && prop.PropertyType.GetGenericArguments()[0].IsAssignableTo(typeof(String)) == false)
            {
                var toDictionary = typeof(DictionaryConvert).GetMethod("CollectionToDictionary", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(prop.PropertyType.GetGenericArguments().First());
                value = Expression.Call(toDictionary, propValue);
            }
            else
            {
                value = Expression.Convert(propValue, typeof(object));
            }
            var addMethod = typeof(Dictionary<string, object>).GetMethod("Add");
            var add = Expression.Call(dictionary, addMethod, Expression.Constant(prop.Name), value);
            body.Add(add);
        }
        body.Add(dictionary);
        var block = Expression.Block(new[] { dictionary }, body);
        return Expression.Lambda<Func<T, Dictionary<string, object>>>(block, param);
    }

    private static List<Dictionary<string, object>> CollectionToDictionary<T>(IEnumerable<T> array)
    {
        var toDictionary = ToDictionaryExpression<T>().Compile();
        int index = 0;
        return array.Select(x => toDictionary(x))
                    .ToList();
    }
}

internal static class ExpressionCahce<T>
{
    public static IDictionary<string, Func<T, Dictionary<string, object>>> Converts { get; set; } = new ConcurrentDictionary<string, Func<T, Dictionary<string, object>>>();
}