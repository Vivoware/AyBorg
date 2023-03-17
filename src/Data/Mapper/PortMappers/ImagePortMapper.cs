using AyBorg.SDK.Common.Models;
using AyBorg.SDK.Common.Ports;
using ImageTorque;

namespace AyBorg.Data.Mapper;

public sealed class ImagePortMapper : IPortMapper<Image>
{
    public object ToNativeValueObject(object value, Type? type = null) => ToNativeValue(value);
    public Image ToNativeValue(object value, Type? type = null) => null!;
    public void Update(IPort port, object value) => ((ImagePort)port).Value = ToNativeValue(value);
    public Port ToModel(IPort port)
    {
        var typedPort = (ImagePort)port;
        Port record = GenericPortMapper.ToRecord(typedPort);
        record.IsLinkConvertable = typedPort.IsLinkConvertable;
        if (typedPort.Value != null)
        {
            record.Value = new CacheImage
            {
                Width = typedPort.Value.Width,
                Height = typedPort.Value.Height,
                PixelFormat = typedPort.Value.PixelFormat,
                OriginalImage = new Image(typedPort.Value)
            };
        }
        else
        {
            record.Value = new CacheImage();
        }
        return record;
    }
}
