/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public interface IModalDialogContext
{
    public IModalOptions? Options { get; }
    public bool Display { get; }
    public bool IsActive { get; }
    public Type? ModalContentType { get; }
    public Action? NotifyRenderRequired { get; set; }

    public Task<ModalResult> ShowAsync<TModal>(IModalOptions options) where TModal : IComponent;
    public Task<ModalResult> ShowAsync(Type control, IModalOptions options);
    public bool Switch<TModal>(IModalOptions options) where TModal : IComponent;
    public bool Switch(Type control, IModalOptions options);
    public void Update(IModalOptions? options = null);
    public void Dismiss();
    public void Close(ModalResult result);
}

