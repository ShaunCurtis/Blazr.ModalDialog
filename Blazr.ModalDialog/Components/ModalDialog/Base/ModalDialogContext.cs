/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public class ModalDialogContext : IModalDialogContext
{
    public IModalOptions? Options { get; protected set; }

    public bool Display { get; protected set; }

    public bool IsActive => this.ModalContentType is not null;

    public Action? NotifyRenderRequired { get; set; }

    private TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

    public Type? ModalContentType {get; private set;} = null;

    public Task<ModalResult> ShowAsync<TModal>(IModalOptions options) where TModal : IComponent
        => this.ShowModalAsync(typeof(TModal), options);

    public Task<ModalResult> ShowAsync(Type control, IModalOptions options)
        => this.ShowModalAsync(control, options);

    public bool Switch<TModal>(IModalOptions options) where TModal : IComponent
        => this.SwitchModal(typeof(TModal), options);

    public bool Switch(Type control, IModalOptions options)
        => this.SwitchModal(control, options);

    public void Update(IModalOptions? options = null)
    {
        this.Options = options ?? this.Options;
        this.NotifyRenderRequired?.Invoke();
    }

    public void Dismiss()
        => this.CloseModal(ModalResult.Cancel());

    public void Close(ModalResult result)
        => this.CloseModal(result);

    private Task<ModalResult> ShowModalAsync(Type control, IModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.Options = options;
        this.ModalContentType = control;
        this.Display = true;
        this.NotifyRenderRequired?.Invoke();
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        return this._ModalTask.Task;
    }

    private bool SwitchModal(Type control, IModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.ModalContentType = control;
        this.Options = options;
        this.NotifyRenderRequired?.Invoke();
        return true;
    }

    private void CloseModal(ModalResult result)
    {
        this.Display = false;
        this.ModalContentType = null;
        this.NotifyRenderRequired?.Invoke();
        _ = this._ModalTask.TrySetResult(result);
    }
}

