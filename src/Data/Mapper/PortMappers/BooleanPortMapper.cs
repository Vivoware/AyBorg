using System.Globalization;
using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Data.Mapper;

public class BooleanPortMapper : IPortMapper<bool>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public bool ToNativeValue(object value, Type? type = null)  => Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    public void Update(IPort port, object value) => ((BooleanPort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (BooleanPort)port;
        return new Port
        {
            Id = port.Id,
            Name = port.Name,
            Direction = port.Direction,
            Brand = port.Brand,
            IsConnected = port.IsConnected,
            IsLinkConvertable = typedPort.IsLinkConvertable,
            Value = typedPort.Value
        };
    }
}
