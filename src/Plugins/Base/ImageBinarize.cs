using System.Globalization;
using Atomy.SDK;
using Atomy.SDK.ImageProcessing;
using Atomy.SDK.Ports;

namespace Atomy.Plugins.Base;

public sealed class ImageBinarize : IStepBody, IDisposable
{
    private readonly ImagePort _inputImagePort = new ImagePort("Image", PortDirection.Input, null!);
    private readonly NumericPort _thresholdPort = new NumericPort("Threshold", PortDirection.Input, 0.5d, 0d, 1d);
    private readonly ImagePort _outputImagePort = new ImagePort("Binarized image", PortDirection.Output, null!);
    private readonly EnumPort _thresholdTypePort = new EnumPort("Mode", PortDirection.Input, BinaryThresholdMode.Lumincance);
    private bool disposedValue;

    public string DefaultName => "Image.Binarize";

    public IEnumerable<IPort> Ports { get; }

    public ImageBinarize()
    {
        Ports = new IPort[]
        {
            _inputImagePort,
            _thresholdTypePort,
            _thresholdPort,
            _outputImagePort
        };
    }

    public Task<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        var threshold = System.Convert.ToSingle(_thresholdPort.Value, CultureInfo.InvariantCulture);
        var mode = (BinaryThresholdMode)_thresholdTypePort.Value;
        _outputImagePort.Value?.Dispose();
        _outputImagePort.Value = _inputImagePort.Value.Binarize(threshold, mode);

        return Task.FromResult(true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _outputImagePort.Dispose();
            }
            disposedValue = true;
        }
    }
}