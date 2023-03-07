using System.Runtime.InteropServices;
using Fenrus.Components.Inputs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.Win32.SafeHandles;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Color
/// </summary>
public partial class InputColor : Input<string>
{
    [Inject] private IJSRuntime jsRuntime { get; set; }

    private bool PickerVisible;

    private double SliderY;
    private double PointerX, PointerY;

    private string BaseColor;
    private int baseR, baseG, baseB;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if(firstRender)
            _ = jsRuntime.InvokeVoidAsync("CreateFenrusColorPicker", 
                DotNetObjectReference.Create(this), 
                this.Uid.ToString());
    }
    
    [JSInvokable("updateValue")]
    public Task UpdateValue(string value)
    {
        this.Value = value;
        StateHasChanged(); 
        return Task.CompletedTask;
    }
    
    private void Toggle()
        => PickerVisible = !PickerVisible;

    private async Task MoveSlider(MouseEventArgs e)
    {
        SliderY = e.OffsetY;
        double percent = e.OffsetY / 150.3d;
        percent = Math.Max(0, Math.Min(1, percent));

        int c1r = 0, c1g = 0, c1b = 0, c2r = 0, c2g = 0, c2b = 0, start = 0, end = 100;
        var gradients = new[]
        {
            (r: 204, g:0, b:0, percent:0), (153, 153, 0, 17),
            (51, 204, 0, 33), (0, 204, 204, 50),
            (0, 0, 204, 66), (204, 0, 204, 83),
            (204, 0, 0, 100)
        };
        for (int i = 0; i < gradients.Length - 1; i++)
        {
            var g1 = gradients[i];
            var g2 = gradients[i + 1];
            if (percent * 100 > g2.Item4)
                continue;

            start = g1.Item4;
            end = g2.Item4;
            c1r = g1.Item1; c1g = g1.Item2; c1b = g1.Item3;
            c2r = g2.Item1; c2g = g2.Item2; c2b = g2.Item3;
            break;
        }
        
        // say percent is .42
        // start = 33, end is 50
        // percent 0 == 33, 100 = 50, 50 = 41.5
        var shifted = (percent * 100) - start; // 42 - 33 = 9
        var newP = shifted / (end - start); // 9 / (50 - 33) == 0.529

        baseR = AdjustColorPercent(c1r, c2r, newP);
        baseG = AdjustColorPercent(c1g, c2g, newP);
        baseB = AdjustColorPercent(c1b, c2b, newP);
        BaseColor = "#" + baseR.ToString("X2") + baseG.ToString("X2") + baseB.ToString("X2");

        CalcuateColor();
    }

    void CalcuateColor()
    {
        var wPercent = PointerX / 211.2d;
        var r = AdjustColorPercent(255, baseR, wPercent);
        var g = AdjustColorPercent(255, baseG, wPercent);
        var b = AdjustColorPercent(255, baseB, wPercent);

        var bPercent = PointerY / 147.1d;
        r = AdjustColorPercent(r,0, bPercent);
        g = AdjustColorPercent(g,0, bPercent);
        b = AdjustColorPercent(b,0, bPercent);
        Value = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }

    int AdjustColorPercent(int zeroC, int hundredC, double percent)
        => Math.Min(255, Math.Max(0, (int)(zeroC + percent * (hundredC - zeroC))));

    void MainPickerClicked(MouseEventArgs e)
    {
        PointerX = e.OffsetX;
        PointerY = e.OffsetY;
        CalcuateColor();
    }
}