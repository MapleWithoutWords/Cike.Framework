using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;

namespace Cike.Data.EFCore.Extensions;

public static class IQueryableDynamicQueryExtensions
{
    /// <summary>
    /// 操作符
    /// </summary>
    public static IDictionary<string, string> Operators = new Dictionary<string, string>();

    /// <summary>
    /// 实体属性关系字典：用于排除掉非实体属性字段查询
    /// </summary>
    private static Dictionary<string, Dictionary<string, Type>> _entityPropertyRelationDic = new Dictionary<string, Dictionary<string, Type>>();

    static IQueryableDynamicQueryExtensions()
    {
        Operators["[like]"] = "like";   //模糊查询
        Operators["[lt]"] = "<";        //小于
        Operators["[gt]"] = ">";        // 大于
        Operators["[lte]"] = "<=";        // 小于等于
        Operators["[gte]"] = ">=";        // 大于等于
        Operators["[ne]"] = "!=";        // 不等于
        Operators["="] = "=";        // 不等于
    }

    public static IQueryable<T> DynamicWhere<T>(this IQueryable<T> query, List<DynamicQuery> list)
    {
        if (list == null || list.Count < 1)
        {
            return query;
        }
        query = query.Where(list.GetWhereParams<T>());
        return query;
    }

    public static string GetWhereParams<T>(this List<DynamicQuery> list)
    {
        if (list == null || list.Count < 1)
        {
            return "1==1";
        }
        var propertyList = GetEntityPropertyList<T>();

        StringBuilder sb = new StringBuilder();
        sb.Append($"1==1 &&");
        if (list == null || list.Count < 1)
        {
            return sb.ToString().TrimEnd('&');
        }
        foreach (var item in list)
        {
            if (item.Value.IsNullOrEmpty())
            {
                continue;
            }
            Regex rg = new Regex(@"(\[.+])");
            string operatorSymbol = "=";
            var matchCollection = rg.Matches(item.Key);
            if (matchCollection.Count > 0)
            {
                foreach (Match mc in matchCollection)
                {
                    if (Operators.ContainsKey(mc.Value))
                    {
                        operatorSymbol = mc.Value;
                    }
                }
            }
            string columnName = item.Key;
            var operatorStr = Operators[operatorSymbol];

            if (operatorSymbol != "=" && !string.IsNullOrEmpty(operatorSymbol))
            {
                columnName = columnName.Replace(operatorSymbol, "");
            }

            var property = propertyList.FirstOrDefault(e => e.Key.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
            if (property.Key.IsNullOrEmpty())
            {
                continue;
            }
            columnName = property.Key;

            if (property.Value == typeof(string) ||
                property.Value == typeof(Guid) ||
                property.Value == typeof(Guid?) ||
                property.Value == typeof(DateTime) ||
                property.Value == typeof(DateTime?))
            {
                item.Value = $"\"{item.Value}\"";
            }

            string condition = $"{columnName}{operatorStr}{item.Value} &&";
            if (operatorStr == "like")
            {
                condition = $"{columnName}.Contains({item.Value}) &&";
            }

            sb.Append(condition);
        }
        return sb.ToString().TrimEnd('&');
    }

    private static Dictionary<string, Type> GetEntityPropertyList<T>()
    {
        var key = typeof(T).FullName!;
        if (_entityPropertyRelationDic.ContainsKey(key))
        {
            return _entityPropertyRelationDic[key];
        }
        var list = new Dictionary<string, Type>();
        foreach (var item in typeof(T).GetProperties())
        {
            list[item.Name] = item.PropertyType;
        }
        _entityPropertyRelationDic[key] = list;
        return list;
    }
}
