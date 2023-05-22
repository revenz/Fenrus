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

    /// <summary>
    /// Gets if this editor is dirty
    /// </summary>
    /// <returns>true if dirty, otherwise false</returns>
    public bool IsDirty()
    {
        foreach (var control in this.Controls)
        {
            if (control.Visible == false)
                continue;
            if (control.Dirty)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Marks all inputs in this editor as clean
    /// </summary>
    public void MarkClean()
    {
        foreach (var control in this.Controls)
        {
            control.Dirty = false;
        }
    }

    /// <summary>
    /// Called after rendering
    /// </summary>
    /// <param name="firstRender">if the first render or not</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if(firstRender)
            this.MarkClean();
        base.OnAfterRender(firstRender);
    }
}