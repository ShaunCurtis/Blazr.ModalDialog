/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public sealed class BsModalOptions: IModalOptions
{
    public string ModalSize { get; set; } = "modal-xl";

    public Dictionary<string, object> ControlParameters { get; } = new Dictionary<string, object>();

    public Dictionary<string, object> OptionsList { get; } = new Dictionary<string, object>();

    public object Data { get; set; } = new();
}

