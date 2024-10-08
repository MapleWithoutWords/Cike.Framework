﻿namespace Cike.Data.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ConnectionStringNameAttribute : Attribute
{
    public string Name { get; }

    public ConnectionStringNameAttribute(string name)
    {
        Name = name;
    }
    public static string GetConnStringName<T>()
    {
        return GetConnStringName(typeof(T));
    }

    public static string GetConnStringName(Type type)
    {
        var nameAttribute = type.GetTypeInfo().GetCustomAttribute<ConnectionStringNameAttribute>();

        if (nameAttribute == null)
        {
            return type.Name.Replace("DbContext", "")!;
        }

        return nameAttribute.Name;
    }
}
