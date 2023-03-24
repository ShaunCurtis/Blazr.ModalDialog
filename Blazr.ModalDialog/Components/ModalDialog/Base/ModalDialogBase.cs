/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public abstract class ModalDialogBase : ComponentBase
{
    public readonly IModalDialogContext Context = new ModalDialogContext();

    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        
        this.Context.NotifyRenderRequired = this.OnRenderRequested;

        return base.SetParametersAsync(ParameterView.Empty);
    }

    private void OnRenderRequested()
        => StateHasChanged();
}

