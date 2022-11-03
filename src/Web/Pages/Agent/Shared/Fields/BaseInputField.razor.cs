using Atomy.SDK.DTOs;
using Atomy.SDK.Ports;
using Microsoft.AspNetCore.Components;

namespace Atomy.Web.Pages.Agent.Shared.Fields;

public partial class BaseInputField : ComponentBase
{
    protected bool _isEditableField = false;

    [Parameter]
    [EditorRequired]
    public PortDto Port { get; set; } = null!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public bool IsEditFieldVisible { get; set; } = false;

    [Parameter]
    public EventCallback<ValueChangedEventArgs> ValueChanged { get; set; }

    protected bool IsDisabled => Port.IsConnected;

    protected override Task OnInitializedAsync()
    {
        if (Port == null)
        {
            throw new ArgumentNullException(nameof(Port));
        }
        _isEditableField = Port.Brand == PortBrand.String
                            || Port.Brand == PortBrand.Numeric
                            || Port.Brand == PortBrand.Rectangle;
        return base.OnInitializedAsync();
    }

    protected virtual async Task NotifyValueChangedAsync(object value)
    {
        await ValueChanged.InvokeAsync(new ValueChangedEventArgs(Port, value));
        IsEditFieldVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    protected virtual void OnEditFieldClicked()
    {
        IsEditFieldVisible = true;
    }
}