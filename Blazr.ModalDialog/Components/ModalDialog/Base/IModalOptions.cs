/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.ModalDialog.Components;

public interface IModalOptions 
{
    public Dictionary<string, object> ControlParameters { get; }

    public Dictionary<string, object> OptionsList { get; }

    public object Data { get; }
}

