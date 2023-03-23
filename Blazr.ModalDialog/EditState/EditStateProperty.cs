using Microsoft.AspNetCore.Components.Forms;

namespace Blazr.ModalDialog.EditState;

public class EditStateProperty
{
    public string Name { get; private set; }
    public object? BaseValue { get; private set; }
    public object? CurrentValue { get; private set; }

    public EditStateProperty(string name, object? value)
    {
        Name = name;
        BaseValue = value;
        CurrentValue= value;
    }

    public void Set(object? value)
        => CurrentValue = value;

    public bool IsDirty => !BaseValue?.Equals(CurrentValue) ?? CurrentValue is not null;
}
