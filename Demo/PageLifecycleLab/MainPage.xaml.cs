namespace PageLifecycleLab;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		LogView.ItemsSource = DemoLog.Lines;
		DemoLog.MessageLogged += OnMessageLogged;
		RefreshStatus();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshStatus();
	}

	private async void OnOpenLifecyclePageClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(LifecyclePage));
	}

	private void OnForceGcClicked(object? sender, EventArgs e)
	{
		// Diagnostic/demo only. Forced GC is not a production memory-management technique.
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		DemoLog.Write($"[Main] Forced diagnostic GC. Last page alive = {PageLifetimeTracker.IsAlive}");
		RefreshStatus();
	}

	private void OnRaiseAppEventClicked(object? sender, EventArgs e)
	{
		DemoLog.Write("[Main] Raising AppEvents.SomethingChanged");
		AppEvents.RaiseSomethingChanged();
		RefreshStatus();
	}

	private void OnClearLogClicked(object? sender, EventArgs e)
	{
		DemoLog.Clear();
		RefreshStatus();
	}

	private void OnMessageLogged(object? sender, string line)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			RefreshStatus();
			if (DemoLog.Lines.Count > 0)
			{
				LogView.ScrollTo(DemoLog.Lines.Count - 1, position: ScrollToPosition.End, animate: false);
			}
		});
	}

	private void RefreshStatus()
	{
		var lastTrackedPageName = PageLifetimeTracker.LastTrackedPageName ?? "-";
		var lastTrackedId = PageLifetimeTracker.LastTrackedId ?? "-";
		var aliveText = PageLifetimeTracker.LastTrackedId is null
			? "Not tracked"
			: PageLifetimeTracker.IsAlive.ToString();

		AliveStatusLabel.Text = $"Last Page Alive: {aliveText}";
		TrackedIdLabel.Text = $"Last tracked page: {lastTrackedPageName} [{lastTrackedId}]";
		SubscriberCountLabel.Text = $"Global event subscriber count: {AppEvents.SubscriberCount}";

		if (aliveText == "True")
		{
			StatusCard.BackgroundColor = Color.FromArgb("#FFE2D7");
			AliveStatusLabel.TextColor = Color.FromArgb("#A63E18");
		}
		else if (aliveText == "False")
		{
			StatusCard.BackgroundColor = Color.FromArgb("#DDF4E7");
			AliveStatusLabel.TextColor = Color.FromArgb("#1D6A43");
		}
		else
		{
			StatusCard.BackgroundColor = Color.FromArgb("#E8EEF5");
			AliveStatusLabel.TextColor = Color.FromArgb("#17324D");
		}
	}
}
