/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public sealed class VanillaModalOptions: IModalOptions
{
    public bool ExitOnBackgroundClick { get; set; } = false;

    public string ModalWidth { get; set; } = "90%";

    public Dictionary<string, object> ControlParameters { get; } = new Dictionary<string, object>();

    public Dictionary<string, object> OptionsList { get; } = new Dictionary<string, object>();

    public object Data { get; set; } = new();
}

