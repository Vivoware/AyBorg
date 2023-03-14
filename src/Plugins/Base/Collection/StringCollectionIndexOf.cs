using System.Collections.ObjectModel;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.Base.Collection;

public sealed class StringCollectionIndexOf : IStepBody
{
    private readonly ILogger<StringCollectionIndexOf> _logger;
    private readonly StringCollectionPort _inputCollection = new("Collection", PortDirection.Input, new ReadOnlyCollection<string>(Array.Empty<string>()));
    private readonly StringPort _inputSearchValue = new("Value", PortDirection.Input, string.Empty);
    private readonly NumericPort _outputIndex = new("Index", PortDirection.Output, 0);

    public string DefaultName => "String.Collection.IndexOf";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.Function };

    public IEnumerable<IPort> Ports { get; }

    public StringCollectionIndexOf(ILogger<StringCollectionIndexOf> logger)
    {
        _logger = logger;
        Ports = new List<IPort>
        {
            _inputCollection,
            _inputSearchValue,
            _outputIndex
        };
    }

    public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
    {
        try
        {
            _outputIndex.Value = _inputCollection.Value.IndexOf(_inputSearchValue.Value);
            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId((int)EventLogType.Plugin), ex, "Could not find index of string {searchValue} in collection", _inputSearchValue.Value);
            return ValueTask.FromResult(false);
        }
    }
}