namespace Fenrus.Pages;

/// <summary>
/// The main blazor App
/// </summary>
public partial class App
{
    private string AccentRgb, AccentColor;
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var settings = DemoHelper.GetDemoUserSettings();
        
        AccentColor = settings.AccentColor;
        AccentRgb = int.Parse(AccentColor[1..3], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[3..5], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[5..7], System.Globalization.NumberStyles.HexNumber);
    }
}