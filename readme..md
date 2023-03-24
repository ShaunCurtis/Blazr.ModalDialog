# A Simple Modal Dialog for Blazor

A web based SPA [Single Page Application] that wants to look like a real application needs modal dialogs.

This article shows how to build a modal dialog container for Blazor Components.  There's a basic vanilla Css and Bootstrap version.

## Code Repository

Coming

## The Implementation

There are three base classes, one interface and one Enum:

1. `IModalDialog`
2. `ModalDialogBase`
3. `IModalOptions`
4. `ModalResult`
5. `ModalResultType`

The following code shows how to open a `WeatherEditForm` within a modal dialog on the `FetchData` page.  You'll see the full implementation later.  The method builds a `ModalOptions` object containing the Uid of the record to display.  It  calls `ShowAsync<WeatherForm>(options)`, defining the form to display and the options for that form, and awaits the returned `Task`.  The `Task` doesn't complete until the modal closes.

```csharp
private async Task EditAsync(Guid uid)
{
    if (_modal is not null)
    {
        var options = new ModalOptions();
        options.ControlParameters.Add("Uid", uid);

        var result = await _modal.ShowAsync<WeatherEditForm>(options);

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

### IModalDialog

`IModalDialog` is the interface defining functiuonality all modal dialogs must imnplement.

```csharp
public interface IModalDialog
{
    public IModalOptions Options { get; }
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

### ModalDialogBase

`ModalDialogBase` provides most of the boilerplate code for Modal Dialog inmplementations.

It consists of a set of methods to show, hide and reset the component content, and captures the `Type` and `ModalOptions` provided in the `Show` methods.

`Show` uses `TaskCompletionSource` to construct a manually controlled Task.  It seta the internal properties with the data supplied, sets `Display` to true and provides the running Task from the  `TaskCompletionSource` to the caller.

```csharp
protected TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

private Task<ModalResult> ShowModalAsync(Type control, IModalOptions options)
{
    if (!(typeof(IComponent).IsAssignableFrom(control)))
        throw new InvalidOperationException("Passed control must implement IComponent");

    this.Options = options;
    this._ModalTask = new TaskCompletionSource<ModalResult>();
    this.ModalContentType = control;
    this.Display = true;
    this.InvokeAsync(StateHasChanged);
    return this._ModalTask.Task;
}
```

`Close` clears the content from the modal and then sets the `TaskCompletionSource` and the Task held by the original caller of `Show` to complete, releasing the calling method to complete.

```csharp
public async void Close(ModalResult result)
{
    this.Display = false;
    this.ModalContentType = null;
    await this.InvokeAsync(StateHasChanged);
    _ = this._ModalTask.TrySetResult(result);
}
```

`Switch` switches the content component and `Upload` provides a mechanism to reload the content component with a different set of options.

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
public abstract class ModalDialogBase : ComponentBase, IModalDialog
{
    public IModalOptions? Options { get; protected set; }

    public bool Display { get; protected set; }

    public bool IsActive => this.ModalContentType is not null;

    protected TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

    protected Type? ModalContentType = null;

    public Task<ModalResult> ShowAsync<TModal>(IModalOptions options) where TModal : IComponent
        => this.ShowModalAsync(typeof(TModal), options);

    public Task<ModalResult> ShowAsync(Type control, IModalOptions options)
        => this.ShowModalAsync(control,options);

    private Task<ModalResult> ShowModalAsync(Type control, IModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.Options = options;
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        this.ModalContentType = control;
        this.Display = true;
        this.InvokeAsync(StateHasChanged);
        return this._ModalTask.Task;
    }

    public Task<bool> SwitchAsync<TModal>(IModalOptions options) where TModal : IComponent
        => this.SwitchModalAsync(typeof(TModal), options);

    public Task<bool> SwitchAsync(Type control, IModalOptions options)
        => this.SwitchModalAsync(control, options);

    private async Task<bool> SwitchModalAsync(Type control, IModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.ModalContentType = control;
        this.Options = options;
        await this.InvokeAsync(StateHasChanged);
        return true;
    }

    public async Task Update(IModalOptions? options = null)
    {
        this.Options = options ?? this.Options;
        await this.InvokeAsync(StateHasChanged);
    }

    public void Dismiss()
        => this.Close(ModalResult.Cancel());

    public async void Close(ModalResult result)
    {
        this.Display = false;
        this.ModalContentType = null;
        await this.InvokeAsync(StateHasChanged);
        _ = this._ModalTask.TrySetResult(result);
    }
}
```


### VanillaModalDialog


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