using LiteDB;

namespace Fenrus.Converters;

/// <summary>
/// Provides serialization and deserialization methods for enum values to be stored as integers.
/// </summary>
public static class EnumSerializer
{
    /// <summary>
    /// Registers custom serializers and deserializers for all enum types in the assembly.
    /// </summary>
    /// <param name="mapper">The BsonMapper instance to register the serializers and deserializers.</param>
    public static void RegisterEnums(BsonMapper mapper)
    {
        BsonMapper.Global.ResolveMember = (type, memberInfo, memberMapper) =>
        {
            if (memberMapper.DataType.IsEnum)
            {
                memberMapper.Serialize = (obj, mapper) => new BsonValue((int)obj);
                memberMapper.Deserialize = (value, mapper) =>
                {
                    if (value.IsString)
                    {
                        var str = value.AsString;
                        if (Enum.TryParse(memberMapper.DataType, str, out var enumValue))
                            return enumValue!;

                        return 0;
                    }
                    else
                    {
                        return Enum.ToObject(memberMapper.DataType, value);
                    }
                };
            }
        };
    }
}
