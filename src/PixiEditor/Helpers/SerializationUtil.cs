using System.Collections;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;

namespace PixiEditor.Helpers;

public static class SerializationUtil
{
    public static IReadOnlyDictionary<Type, string> EmptyContentWellKnownTypes =
        new Dictionary<Type, string>()
        {
            { typeof(Painter), "PixiEditor.Painter" },
            { typeof(Paintable), "PixiEditor.Paintable" },
            { typeof(VectorPath), "PixiEditor.VectorPath" }
        };

    public static bool IsFilePreVersion((string serializerName, string serializerVersion) serializerData,
        Version minSupportedVersion)
    {
        return serializerData.serializerName == "PixiEditor"
               && Version.TryParse(serializerData.serializerVersion, out Version version)
               && version < minSupportedVersion;
    }

    public static object SerializeObject(object? value, SerializationConfig config,
        IReadOnlyList<SerializationFactory> allFactories)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Delegate del)
        {
            value = del.DynamicInvoke(FuncContext.NoContext);
            if (value is ShaderExpressionVariable expressionVariable)
            {
                value = expressionVariable.GetConstant();
            }
        }

        var factory = allFactories.FirstOrDefault(x => x.OriginalType == value.GetType());
        if (factory == null)
        {
            factory = allFactories.FirstOrDefault(x => value.GetType().IsAssignableTo(x.OriginalType));
        }

        if (factory != null)
        {
            factory.Config = config;
            return factory.Serialize(value);
        }

        if (value.GetType().IsValueType || value is string || value is object[])
        {
            return value;
        }

        throw new ArgumentException(
            $"Type {value.GetType()} is not serializable and appropriate serialization factory was not found.");
    }

    public static string? GetWellKnownSerializationTypeName(Type type, IReadOnlyList<SerializationFactory> allFactories)
    {
        var factory = allFactories.FirstOrDefault(x => x.OriginalType == type);
        if (factory == null)
        {
            factory = allFactories.FirstOrDefault(x => type.IsAssignableTo(x.OriginalType));
        }

        if (factory != null)
        {
            return factory.DeserializationId;
        }

        if (type.IsPrimitive || type == typeof(string))
        {
            return type.Name;
        }

        if (EmptyContentWellKnownTypes.TryGetValue(type, out string? name))
        {
            return name;
        }

        return null;
    }

    public static object Deserialize(object value, SerializationConfig config,
        IReadOnlyList<SerializationFactory> allFactories,
        (string serializerName, string serializerVersion) serializerData)
    {
        if (IsComplexObject(value))
        {
            object[] arr = ((IEnumerable<object>)value).ToArray();
            string id = (string)arr.First();
            object data = arr.Last();
            var factory = allFactories.FirstOrDefault(x => x.DeserializationId == id);

            if (factory != null)
            {
                factory.Config = config;
                try
                {
                    return factory.Deserialize(data is Dictionary<object, object> processableDict
                        ? ToDictionary(processableDict)
                        : data, serializerData);
                }
                catch (Exception e)
                {
                    return value;
                }
            }
        }

        return value;
    }


    public static Dictionary<string, object> DeserializeDict(Dictionary<string, object> data,
        SerializationConfig config, List<SerializationFactory> allFactories,
        (string serializerName, string serializerVersion) serializerData)
    {
        var dict = new Dictionary<string, object>();

        foreach (var (key, value) in data)
        {
            if (value is object[] objArr && objArr.Length > 0)
            {
                if (IsComplexObject(value))
                {
                    dict[key] = Deserialize(value, config, allFactories, serializerData);
                }
                else
                {
                    var deserialized = Deserialize(objArr[0], config, allFactories, serializerData);
                    var targetArr = Array.CreateInstance(deserialized.GetType(), objArr.Length);
                    targetArr.SetValue(deserialized, 0);

                    for (int i = 1; i < objArr.Length; i++)
                    {
                        targetArr.SetValue(Deserialize(objArr[i], config, allFactories, serializerData), i);
                    }

                    dict[key] = targetArr;
                }
            }
            else
            {
                dict[key] = Deserialize(value, config, allFactories, serializerData);
            }
        }

        return dict;
    }

    private static bool IsComplexObject(object value)
    {
        // SerializedObject signature
        return value is IEnumerable<object> enumerable && enumerable.Count() == 2;
    }

    private static Dictionary<string, object> ToDictionary(Dictionary<object, object> data)
    {
        // input data is probably Dictionary<object, object> with KeyValuePair keys (where key as type is object, but actually is string)
        // note that this depends if serialized type uses string keys or int

        var dict = new Dictionary<string, object>();

        foreach (KeyValuePair<object, object> item in data)
        {
            dict.Add((string)item.Key, item.Value);
        }

        return dict;
    }

    public static Type? GetTypeForWellKnownTypeName(string type, IReadOnlyList<SerializationFactory> allFactories)
    {
        var firstEmptyContentType = EmptyContentWellKnownTypes.FirstOrDefault(x => x.Value == type).Key;
        if (firstEmptyContentType != null)
            return firstEmptyContentType;

        var factory = allFactories.FirstOrDefault(x => x.DeserializationId == type);
        if (factory != null)
        {
            return factory.OriginalType;
        }

        Type primitiveType = Type.GetType($"System.{type}", false, true);
        if (primitiveType != null)
        {
            return primitiveType;
        }

        return null;
    }
}
