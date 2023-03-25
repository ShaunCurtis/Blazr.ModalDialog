# A Simple Modal Dialog for Blazor

A web based SPA [Single Page Application] needs modal dialogs if it wants to look like a real application.

This article demonstrates how to build a simple generic modal dialog container for Blazor Components.  There's a basic vanilla Css and Bootstrap version.

## Code Repository

[Modal Dialog Repository](https://github.com/ShaunCurtis/Blazr.ModalDialog)

## The Implementation

There are four classes, two interfaces and an Enum:

1. `IModalOptions`
1. `IModalDialogContext`
1. `ModalOptions`
1. `ModalResult`
1. `ModalResultType`
1. `ModalDialogContext`
1. `ModalDialogBase`

The following code shows how to open a `WeatherEditForm` within a modal dialog on the `FetchData` page.  You'll see the full implementation later.  The method builds a `IModalOptions` object containing the Uid of the record to display.  It  calls `ShowAsync<WeatherForm>(options)`, defining the form to display and the options for that form, and awaits the returned `Task`.  The `Task` doesn't complete until the modal closes.

```csharp
private async Task EditAsync(Guid uid)
{
    if (_modal is not null)
    {
        var options = new BsModalOptions();
        options.ControlParameters.Add("Uid", uid);

        var result = await _modal.Context.ShowAsync<WeatherEditForm>(options);
        // Code to run after the Dialog closes 
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

`IModalOptions` defines three ways to pass data into the dialog.  Implementations are specific to the modal dialog implementation.

```csharp
public interface IModalOptions 
{
    public Dictionary<string, object> ControlParameters { get; }
    public Dictionary<string, object> OptionsList { get; }
    public object Data { get; }
}
```

### ModalOptions

A basic implementation of `IModalOptions`.

```csharp
public class ModalOptions: IModalOptions
{
    public Dictionary<string, object> ControlParameters { get; } = new Dictionary<string, object>();

    public Dictionary<string, object> OptionsList { get; } = new Dictionary<string, object>();

    public object Data { get; set; } = new();
}
```


### ModalResult

`ModalResult` provides a structured method to return status and data back to the caller.

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

`ModalDialogContext` encapsulates state and state management for a modal dialog component in a context class.  The context is cascaded not the component.

`IModalDialogContext` defines the interface.

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

`ModalDialogContext` implements `IModalDialogContext`,  providing the boilerplate code for Modal Dialog implementations.

It consists of properties to maontain state and methods to show, hide and reset the component content.

`Show`:
1. Ensures the passed type is a component i.e implements `IComponent`.
2. Sets the state.
3. Invokes the callback to notify the component to render: this will show the dialog framework and create the content component.
4. Uses a `TaskCompletionSource` to construct a manual active Task and passes the task back to the caller to `await`.

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
2. Invokes the callback to notify the component to render: this will hide the dialog framework and destroy the content component.
3. Sets the `TaskCompletionSource` to complete.  If the caller awaited `Show`, the call method will now run to completion.

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
2. Invokes the callback to notify the component to render: this will show the dialog framework with the new content component.

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
```

### ModalDialogBase

`ModalDialogBase` implements the boilerplate plate code for modal dialog component.

It creates an instance of `ModalDialogContext` and sets the callback in `SetParametersAsync` rather that in `OnInitialized` to ensure it isn't overridden by inheriting classes.

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

### BsModelDialog

The custom `IModalOptions`:

```csharp
public sealed class BsModalOptions: IModalOptions
{
    public string ModalSize { get; set; } = "modal-xl";

    public Dictionary<string, object> ControlParameters { get; } = new Dictionary<string, object>();

    public Dictionary<string, object> OptionsList { get; } = new Dictionary<string, object>();

    public object Data { get; set; } = new();
}
```

*BsModalDialog.razor*

```csharp
@namespace Blazr.ModalDialog.Components
@inherits ModalDialogBase

@if (this.Context.Display)
{
    <CascadingValue Value="(IModalDialogContext)this.Context">
        <div class="modal show-modal" tabindex="-1">
            <div class="modal-dialog @this.Size">
                <div class="modal-content">
                    <div class="modal-body">
                        <DynamicComponent Type=this.Context.ModalContentType Parameters=this.Context.Options?.ControlParameters />
                    </div>
                </div>
            </div>
        </div>
    </CascadingValue>
}

@code {
    private BsModalOptions modalOptions => this.Context.Options as BsModalOptions ?? new();
        
    protected string Size => modalOptions.ModalSize;
}
```

and *BsModalDialog.razor.css*:

```css
.modal-body {
    padding: 0;
}

.show-modal {
    display: block;
    background-color: rgb(0,0,0,0.6);
}
```

## Demonstration

The demonstration modifies the `FetchData` page, demonstrating a modal dialog editor for the weather forecasts.  You can view all the code in the repository.

An edit form for a `WeatherForecast` record.

1. Capture the cascaded `IModalDialogContext`.
1. As the form is designed to run in a modal dialog context, throw an expection if there's no cascaded `IModalDialogContext`.
1. The form uses `EditStateTracker` which tracks the edir state and is detailed here [Blazr.EditStateTracker](https://github.com/ShaunCurtis/Blazr.EditStateTracker).
1. The form interacts with the modal context in *Save* and *Close*.


*WeatherEditForm*
```csharp
@inject WeatherForecastService DataService

<div class="p-3">

    <div class="mb-3 display-6 border-bottom">
        Weather Forecast Editor
    </div>

    <EditForm Model=this.model OnSubmit=this.SaveAsync>

        <DataAnnotationsValidator/>
        <EditStateTracker LockNavigation EditStateChanged=this.OnEditStateChanged />

        <div class="mb-3">
            <label class="form-label">Date</label>
            <InputDate class="form-control" @bind-Value=this.model.Date />
        </div>

        <div class="mb-3">
            <label class="form-label">Temperature &deg;C</label>
            <InputNumber class="form-control" @bind-Value=this.model.TemperatureC />
        </div>

        <div class="mb-3">
            <label class="form-label">Summary</label>
            <InputSelect class="form-select" @bind-Value=this.model.Summary>
                @if (model.Summary is null)
                {
                    <option disbabled selected value="null"> -- Select a Summary -- </option>
                }
                @foreach (var summary in this.DataService.Summaries)
                {
                    <option value="@summary">@summary</option>
                }
            </InputSelect>
        </div>

        <div class="mb-3 text-end">
            <button disabled="@(!_isDirty)" type="submit" class="btn btn-primary" @onclick=SaveAsync>Save</button>
            <button disabled="@_isDirty" type="button" class="btn btn-dark" @onclick=Close>Exit</button>
        </div>

    </EditForm>

</div>

<div class="bg-dark text-white m-4 p-2">
    <pre>Date : @this.model.Date</pre>
    <pre>Temperature &deg;C : @this.model.TemperatureC</pre>
    <pre>Summary: @this.model.Summary</pre>
    <pre>State: @(_isDirty ? "Dirty" : "Clean")</pre>
</div>

@code {
    [Parameter] public Guid Uid { get; set; }
    [CascadingParameter] private IModalDialogContext? Modal { get; set; }

    private WeatherForecast model = new();
    private bool _isDirty;

    protected override async Task OnInitializedAsync()
    {
        ArgumentNullException.ThrowIfNull(Modal);
        model = await this.DataService.GetForecastAsync(this.Uid) ?? new() { Date = DateOnly.FromDateTime(DateTime.Now), TemperatureC = 10 };
    }
    
    private void OnEditStateChanged(bool isDirty)
        => _isDirty = isDirty;

    private async Task SaveAsync()
    {
        await this.DataService.SaveForecastAsync(model);
        this.Modal?.Close(ModalResult.OK());
    }

    private void Close()
        =>  this.Modal?.Close(ModalResult.OK());
}
```

And `FetchData`.

1. Adds an Edit button to each row.
2. Adds the `BsModalDialog` component to the page.
3. Calls `ShowAsync` on the modal component to open the modal dialog with the Edit Form.

```csharp
@page "/fetchdata"
@using Blazr.ModalDialog.Data
@inject WeatherForecastService ForecastService

<PageTitle>Weather forecast</PageTitle>

<h1>Weather forecast</h1>

<p>This component demonstrates fetching data from a service.</p>

<table class="table">
    <thead>
        <tr>
            <th>Date</th>
            <th>Temp. (C)</th>
            <th>Temp. (F)</th>
            <th>Summary</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var forecast in forecasts)
        {
            <tr>
                <td>@forecast.Date.ToShortDateString()</td>
                <td>@forecast.TemperatureC</td>
                <td>@forecast.TemperatureF</td>
                <td>@forecast.Summary</td>
                <td><button class="btn btn-sm btn-primary" @onclick="() => EditAsync(forecast.Uid)">Edit</button></td>
            </tr>
        }
    </tbody>
</table>

<BsModalDialog @ref=_modal />

@code {
    private IEnumerable<WeatherForecast> forecasts = Enumerable.Empty<WeatherForecast>();
    private BsModalDialog? _modal;

    protected override async Task OnInitializedAsync()
    {
        forecasts = await ForecastService.GetForecastAsync();
    }

    private async Task EditAsync(Guid uid)
    {
        if (_modal is not null)
        {
            var options = new BsModalOptions();
            options.ControlParameters.Add("Uid", uid);

            var result = await _modal.Context.ShowAsync<WeatherEditForm>(options);
        }
    }
}
```

## Wrap Up



## History

1. 19-Nov-2020: Initial version
2. 25-Nov-2023: Revision 1