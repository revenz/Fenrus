using Fenrus.Components.Inputs;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

public class EditorForm:ComponentBase
{
    /// <summary>
    /// Validates all the controls in the editor
    /// </summary>
    /// <returns>true if the editor is valid, otherwise false</returns>
    public async Task<bool> Validate()
    {
        bool valid = true;
        foreach (var ctrl in Controls)
        {
            valid &= await ctrl.Validate();
        }
        return valid;
    }

    private readonly List<IInput> Controls = new ();

    /// <summary>
    /// Registers a control with the editor
    /// </summary>
    /// <param name="control">the control</param>
    public void RegisterControl(IInput control)
    {
        if (Controls.Contains(control) == false)
            Controls.Add(control);
    }

    /// <summary>
    /// Unregisters a control with the editor
    /// </summary>
    /// <param name="control">the control</param>
    public void UnregisterControl(IInput control)
    {
        if (Controls.Contains(control))
            Controls.Remove(control);
    }
}