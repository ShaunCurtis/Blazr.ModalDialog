/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.ModalDialog.Components;

public interface IModalDialog
{
    public IModalOptions? Options { get; }

    public bool IsActive { get; }

    public bool Display { get; }

    public Task<ModalResult> ShowAsync<TModal>(IModalOptions options) where TModal : IComponent;

    public Task<ModalResult> ShowAsync(Type control, IModalOptions options);

    public Task<bool> SwitchAsync<TModal>(IModalOptions options) where TModal : IComponent;

    public Task<bool> SwitchAsync(Type control, IModalOptions options);

    public void Dismiss();

    public void Close(ModalResult result);

    public void Update(IModalOptions? options = null);
}

