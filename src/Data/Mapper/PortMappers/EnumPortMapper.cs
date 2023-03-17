using System.Text.Json;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public sealed class EnumPortMapper : IPortMapper<System.Enum>
{
    public object ToNativeObject(object value, Type? type = null) => ToNativeType(value);
    public System.Enum ToNativeType(object value, Type? type = null)
    {
        EnumRecord record;
        if (value is System.Enum enumValue)
        {
            record = new EnumRecord
            {
                Name = enumValue.ToString(),
                Names = System.Enum.GetNames(enumValue.GetType())
            };
        }
        else if (value is SDK.Common.Models.Enum enumBinding)
        {
            record = new EnumRecord
            {
                Name = enumBinding.Name!,
                Names = enumBinding.Names!
            };
        }
        else
        {
            record = JsonSerializer.Deserialize<EnumRecord>(value.ToString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        return (System.Enum)System.Enum.Parse(type!, record.Name);
    }
    public void Update(IPort port, object value)
    {
        var typedPort = (EnumPort)port;
        typedPort.Value = ToNativeType(value, typedPort.Value.GetType());
    }

    public Port ToRecord(IPort port)
    {
        var typedPort = (EnumPort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        record.Value = typedPort.Value;
        return record;
    }
}
