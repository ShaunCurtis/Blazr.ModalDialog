/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public abstract class ModalDialogBase : ComponentBase
{
    public readonly IModalDialogContext Context = new ModalDialogContext();

    public ModalDialogBase()
        => this.Context.NotifyRenderRequired = this.OnRenderRequested;

    private void OnRenderRequested()
        => StateHasChanged();
}

