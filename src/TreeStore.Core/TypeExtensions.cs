namespace TreeStore.Core;

public static class TypeExtensions
{
    public static bool IsDictionaryWithStringKey(this Type type)
    {
        if (!type.ImplementsGenericDefinition(typeof(IDictionary<,>), out var implementingType))
            return false;

        if (implementingType.GetGenericArguments().First().Equals(typeof(string)))
            return true;

        return false;
    }

    public static object GetValues(IDictionary<string, object> underlying)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Verifies that the <paramref name="type"/> implements the <paramref name="genericInterfaceDefinition"/> and
    /// extracts the type combination in <paramref name="implementingType"/>.
    /// Taken from https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/ReflectionUtils.cs
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericInterfaceDefinition"></param>
    /// <param name="implementingType"></param>
    /// <returns></returns>
    public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
    {
        if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
        {
            implementingType = default;
            return false;
        }

        if (type.IsInterface)
        {
            if (type.IsGenericType)
            {
                Type interfaceDefinition = type.GetGenericTypeDefinition();

                if (genericInterfaceDefinition == interfaceDefinition)
                {
                    implementingType = type;
                    return true;
                }
            }
        }

        foreach (Type i in type.GetInterfaces())
        {
            if (i.IsGenericType)
            {
                Type interfaceDefinition = i.GetGenericTypeDefinition();

                if (genericInterfaceDefinition == interfaceDefinition)
                {
                    implementingType = i;
                    return true;
                }
            }
        }

        implementingType = default;
        return false;
    }
}