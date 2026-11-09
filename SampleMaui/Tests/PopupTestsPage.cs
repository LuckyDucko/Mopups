namespace SampleMaui.Tests;

/// <summary>Runs <see cref="PopupWindowTests"/> on the device. Results go to the page and to the debug output (tag MOPUPTEST).</summary>
public class PopupTestsPage : ContentPage
{
    readonly Label summary = new() { Text = "not run", AutomationId = "summary", FontAttributes = FontAttributes.Bold };
    readonly Label details = new() { AutomationId = "details", FontSize = 12 };
    readonly Button runButton;

    public PopupTestsPage()
    {
        Title = "Popup window tests";
        BackgroundColor = Colors.White;

        runButton = new Button { Text = "Run all", AutomationId = "runAll" };
        runButton.Clicked += async (_, _) => await RunAsync();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout { Padding = 16, Spacing = 8, Children = { runButton, summary, details } }
        };
    }

    async Task RunAsync()
    {
        runButton.IsEnabled = false;
        summary.Text = "running...";
        details.Text = string.Empty;

        var results = await new PopupWindowTests().RunAllAsync();

        var failed = results.Count(r => r.Error is not null);
        summary.Text = failed == 0 ? $"PASSED {results.Count}/{results.Count}" : $"FAILED {failed} of {results.Count}";
        details.Text = string.Join("\n", results.Select(r => r.Error is null ? $"PASS  {r.Name}" : $"FAIL  {r.Name}\n      {r.Error}"));
        runButton.IsEnabled = true;
    }
}
