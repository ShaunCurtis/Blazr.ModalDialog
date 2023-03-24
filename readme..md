# A Simple Modal Dialog for Blazor

A web based SPA [Single Page Application] that wants to look like a real application needs modal dialogs.

This article shows how to build a modal dialog container for Blazor Components.  There's a basic vanilla Css and Bootstrap version.

## Code Repository

Coming

## The Implementation

There are three classes, two interface and an Enum:

1. `IModalOptions`
2. `IModalDialogContext`
3. `ModalResult`
4. `ModalResultType`
5. `ModalDialogContext`
6. `ModalDialogBase`

The following code shows how to open a `WeatherEditForm` within a modal dialog on the `FetchData` page.  You'll see the full implementation later.  The method builds a `ModalOptions` object containing the Uid of the record to display.  It  calls `ShowAsync<WeatherForm>(options)`, defining the form to display and the options for that form, and awaits the returned `Task`.  The `Task` doesn't complete until the modal closes.

```csharp
private async Task EditAsync(Guid uid)
{
    if (_modal is not null)
    {
        var options = new ModalOptions();
        options.ControlParameters.Add("Uid", uid);

        var result = await _modal.Context.ShowAsync<WeatherEditForm>(options);

        // Code here doesn't run until the dialog closes
    }
}
```

The form calls `Close(modal result)` which completes the `Task` and `EditAsync` runs to completion.

```csharp
private void Close()
{
    this.Modal?.Close(ModalResult.OK());
}
```

### IModalOptions

`IModalOptions` defines three ways if passing data into `TModel`.  Implementations are specific to the modal dialog impklementation.

```csharp
public interface IModalOptions 
{
    public Dictionary<string, object> ControlParameters { get; }
    public Dictionary<string, object> OptionsList { get; }
    public object Data { get; }
}
```

### ModalResult

`ModalResult` provides a structure way to return both status and data back to the caller.

```csharp
public sealed class ModalResult
{
    public ModalResultType ResultType { get; private set; } = ModalResultType.NoSet;

    public object? Data { get; set; } = null;

    public static ModalResult OK() => new ModalResult() { ResultType = ModalResultType.OK };

    //... lots of static constructors
}
```

And `ModalResultType`:

```csharp
public enum ModalResultType { NoSet, OK, Cancel, Exit }
```

### IModalDialogContext

`IModalDialogContext` is the interface defining the functionality for state and state management of the modal dialog.  The context is maintained in a separate class to the modal dialog component.  We can cascade the context without having to cascade the whole component.

```csharp
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
```

### ModalDialogContext

`ModalDialogContext` provides most of the boilerplate code for Modal Dialog implementations.

It consists of a set of methods to show, hide and reset the component content, and captures the `Type` and `ModalOptions` provided in the `Show` methods.

`Show`:
1. Sets the state.
2. Notifies the component to render: this will show the dialog framework and create the content component.
3. Uses a `TaskCompletionSource` to construct a manually controlled Task and provides the Task (in the not completed state) to the caller to `await`.

```csharp
protected TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

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
```

`Close`:

1. Clears the state.
2. Notifies the component to render: this will hide the dialog framework and destroy the content component.
3. Sets the `TaskCompletionSource` to complete.  This releases the caller of `Show` (if they awaited) to complete execution.

```csharp
private void CloseModal(ModalResult result)
{
    this.Display = false;
    this.ModalContentType = null;
    this.NotifyRenderRequired?.Invoke();
    _ = this._ModalTask.TrySetResult(result);
}
```

`Switch`:

1. Sets the state.
2. Notifies the component to render: this will show the dialog framework with the new content component.

```csharp
private async Task<bool> SwitchModalAsync(Type control, IModalOptions options)
{
    if (!(typeof(IComponent).IsAssignableFrom(control)))
        throw new InvalidOperationException("Passed control must implement IComponent");

    this.ModalContentType = control;
    this.Options = options;
    await this.InvokeAsync(StateHasChanged);
    return true;
}
```

The full class:

```csharp
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
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        this.ModalContentType = control;
        this.Display = true;
        this.NotifyRenderRequired?.Invoke();
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
```

### ModalDialogBase

`ModalDialogBase` implements the boilerplate plate code for modal dialog implementations.

It sets the callback in `SetParametersAsync` rather that in `OnInitlized` to ensure it isn't overridden by inheriting classes.

```csharp
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
```

### VanillaModalDialog

`VanillaModalDialog` provides a basic modal dialog wrapper around the component.  It has:

1. A clickable background.
2. Configurable width.
3. Uses `DynamicComponent` to render the requested component.

```csharp
@namespace Blazr.ModalDialog.Components
@inherits ModalDialogBase
@implements IModalDialog

@if (this.Display)
{
    <CascadingValue Value="(IModalDialog)this">
        <div class="base-modal-background" @onclick="OnBackClick">
            <div class="base-modal-content" style="@this.Width" @onclick:stopPropagation="true">
                <DynamicComponent Type=this.ModalContentType Parameters=this.Options?.ControlParameters />
            </div>
        </div>
    </CascadingValue>
}

@code {
    private VanillaModalOptions modalOptions => this.Options as VanillaModalOptions ?? new();

    protected string Width
        => string.IsNullOrWhiteSpace(modalOptions.ModalWidth) ? string.Empty : $"width:{modalOptions.ModalWidth}";

    private void OnBackClick()
    {
        if (modalOptions.ExitOnBackgroundClick)
            this.Close(ModalResult.Exit());
    }
}
```

*VanillaModalDialog.razor.css*

```css
div.base-modal-background {
    display: block;
    position: fixed;
    z-index: 101; /* Sit on top */
    left: 0;
    top: 0;
    width: 100%; /* Full width */
    height: 100%; /* Full height */
    overflow: auto; /* Enable scroll if needed */
    background-color: rgb(0,0,0); /* Fallback color */
    background-color: rgba(0,0,0,0.4); /* Black w/ opacity */
}

div.base-modal-content {
    background-color: #fefefe;
    margin: 10% auto;
    padding: 10px;
    border: 2px solid #888;
    width: 90%;
}
```




## History

1. 19th November, 2020: Initial version