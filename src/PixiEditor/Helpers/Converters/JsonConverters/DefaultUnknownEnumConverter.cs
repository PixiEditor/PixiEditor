using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PixiEditor.Helpers.Converters.JsonConverters;

public class DefaultUnknownEnumConverter : StringEnumConverter
{
    /// <summary>
    /// The default value used to fallback on when a enum is not convertable.
    /// </summary>
    private readonly int defaultValue;

    /// <inheritdoc />
    /// <summary>
    /// Default constructor. Defaults the default value to 0.
    /// </summary>
    public DefaultUnknownEnumConverter()
    {

    }

    /// <inheritdoc />
    /// <summary>
    /// Sets the default value for the enum value.
    /// </summary>
    /// <param name="defaultValue">The default value to use.</param>
    public DefaultUnknownEnumConverter(int defaultValue)
    {
        this.defaultValue = defaultValue;
    }

    /// <inheritdoc />
    /// <summary>
    /// Reads the provided JSON and attempts to convert using StringEnumConverter. If that fails set the value to the default value.
    /// </summary>
    /// <param name="reader">Reads the JSON value.</param>
    /// <param name="objectType">Current type that is being converted.</param>
    /// <param name="existingValue">The existing value being read.</param>
    /// <param name="serializer">Instance of the JSON Serializer.</param>
    /// <returns>The deserialized value of the enum if it exists or the default value if it does not.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return Enum.Parse(objectType, $"{defaultValue}");
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Validates that this converter can handle the type that is being provided.
    /// </summary>
    /// <param name="objectType">The type of the object being converted.</param>
    /// <returns>True if the base class says so, and if the value is an enum and has a default value to fall on.</returns>
    public override bool CanConvert(Type objectType)
    {
        return base.CanConvert(objectType) && objectType.GetTypeInfo().IsEnum && Enum.IsDefined(objectType, defaultValue);
    }
}
