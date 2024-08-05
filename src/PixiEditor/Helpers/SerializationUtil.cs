﻿using System.Collections;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;

namespace PixiEditor.Helpers;

public static class SerializationUtil
{
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
        }

        var factory = allFactories.FirstOrDefault(x => x.OriginalType == value.GetType());

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

    public static object Deserialize(object value, SerializationConfig config,
        IReadOnlyList<SerializationFactory> allFactories)
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
                return factory.Deserialize(data is Dictionary<object, object> processableDict
                    ? ToDictionary(processableDict)
                    : data);
            }
        }

        return value;
    }


    public static Dictionary<string, object> DeserializeDict(Dictionary<string, object> data,
        SerializationConfig config, List<SerializationFactory> allFactories)
    {
        var dict = new Dictionary<string, object>();

        foreach (var (key, value) in data)
        {
            if (value is object[] objArr && objArr.Length > 0)
            {
                var deserialized = Deserialize(objArr[0], config, allFactories);
                var targetArr = Array.CreateInstance(deserialized.GetType(), objArr.Length);
                targetArr.SetValue(deserialized, 0);
                
                for (int i = 1; i < objArr.Length; i++)
                {
                    targetArr.SetValue(Deserialize(objArr[i], config, allFactories), i);
                }
                
                dict[key] = targetArr;
            }
            else
            {
                dict[key] = Deserialize(value, config, allFactories);
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
}