using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MQTTnet;
using Atomy.SDK;
using Atomy.SDK.ImageProcessing;
using Atomy.SDK.MQTT;
using Atomy.SDK.Ports;

namespace Atomy.Plugins.Base.MQTT;

public sealed class MqttImageReceive : BaseMqttReceiveStep
{
    private readonly ImagePort _imagePort = new ImagePort("Image", PortDirection.Output, null!);
    private readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();

    public override string DefaultName => "MQTT.Image.Receive";

    public  MqttImageReceive(ILogger<MqttImageReceive> logger, IMqttClientProvider mqttClientProvider)
        : base(logger, mqttClientProvider)
    {
        _ports.Add(_imagePort);
    }

    protected override void OnMessageReceived(MqttApplicationMessage message)
    {
        if(message.Payload == null)
        {
            _logger.LogWarning("Received message with null payload");
            return;
        }

        _logger.LogTrace("Received message from topic {topic}", message.Topic);
        using var stream = _memoryStreamManager.GetStream(message.Payload);

        var image = Image.Load(stream);
        _imagePort.Value?.Dispose();
        _imagePort.Value = image;
        _hasNewMessage = true;
    }
}