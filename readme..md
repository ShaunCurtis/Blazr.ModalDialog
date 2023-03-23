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

    public static ModalResult Exit() => new ModalResult() { ResultType = ModalResultType.Exit };

    public static ModalResult Cancel() => new ModalResult() { ResultType = ModalResultType.Cancel };

    public static ModalResult OK(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.OK };

    public static ModalResult Exit(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.Exit };

    public static ModalResult Cancel(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.Cancel };
}
```

And `ModalResultType`:

```csharp
public enum ModalResultType { NoSet, OK, Cancel, Exit }
```

### ModalDialogBase

`ModalDialogBase` provides most of the boilerplate code for Modal Dialog inmplementations.

It consists of a set of methods to show, hide and reset the component content.

It captures the `Type` and `ModalOptions` provided in the `Show` methods

```csharp
public abstract class ModalDialogBase : ComponentBase, IModalDialog
{
    public IModalOptions Options { get; protected set; } = new ModalOptions();

    public bool Display { get; protected set; }

    public bool IsActive => this.ModalContentType is not null;

    protected TaskCompletionSource<ModalResult> _ModalTask { get; set; } = new TaskCompletionSource<ModalResult>();

    protected Type? ModalContentType = null;

    public Task<ModalResult> ShowAsync<TModal>(ModalOptions options) where TModal : IComponent
    {
        this.ModalContentType = typeof(TModal);
        this.Options = options ??= this.Options;
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        this.Display = true;
        InvokeAsync(StateHasChanged);
        return this._ModalTask.Task;
    }

    public Task<ModalResult> ShowAsync(Type control, ModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.Options = options ??= this.Options;
        this._ModalTask = new TaskCompletionSource<ModalResult>();
        this.ModalContentType = control;
        this.Display = true;
        InvokeAsync(StateHasChanged);
        return this._ModalTask.Task;
    }

    public async Task<bool> SwitchAsync<TModal>(ModalOptions options) where TModal : IComponent
    {
        this.ModalContentType = typeof(TModal);
        this.Options = options ??= this.Options;
        await InvokeAsync(StateHasChanged);
        return true;
    }

    public async Task<bool> SwitchAsync(Type control, ModalOptions options)
    {
        if (!(typeof(IComponent).IsAssignableFrom(control)))
            throw new InvalidOperationException("Passed control must implement IComponent");

        this.ModalContentType = control;
        this.Options = options ??= this.Options;
        await InvokeAsync(StateHasChanged);
        return true;
    }

    /// <summary>
    /// Method to update the state of the display based on UIOptions
    /// </summary>
    /// <param name="options"></param>
    public void Update(ModalOptions? options = null)
    {
        this.Options = options ??= this.Options;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Method called by the dismiss button to close the dialog
    /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
    /// </summary>
    public async void Dismiss()
    {
        _ = this._ModalTask.TrySetResult(ModalResult.Cancel());
        await Reset();
    }

    /// <summary>
    /// Method called by child components through the cascade value of this component
    /// sets the task to complete, show to false and renders the component (which hides it as show is false!)
    /// </summary>
    /// <param name="result"></param>
    public async void Close(ModalResult result)
    {
        _ = this._ModalTask.TrySetResult(result);
        await Reset();
    }

    private async Task Reset()
    {
        this.Display = false;
        this.ModalContentType = null;
        await InvokeAsync(StateHasChanged);
    }
}
```


### VanillaModalDialog


```csharp
```




