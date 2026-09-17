using Microsoft.Maui.Handlers;

namespace PageLifecycleLab;

public partial class LifecyclePage : ContentPage
{
	private const string PageName = nameof(LifecyclePage);
	private static readonly bool CleanupStaticEventOnDisappearing = false;
	private readonly string _id = Guid.NewGuid().ToString("N")[..6];
    private IDispatcherTimer? _timer;
    private int _timerTickCount;
	private bool _staticEventSubscribed;
	private bool _nativeEventSubscribed;

#if ANDROID
	private Android.Views.View.IOnFocusChangeListener? _androidFocusListener;
#endif

#if IOS
	private EventHandler? _iosEditingDidBeginHandler;
#endif

	public LifecyclePage()
	{
		ConstructionScope.CurrentPageId = _id;
		InitializeComponent();
		ConstructionScope.CurrentPageId = null;

		PageNameLabel.Text = PageName;
		InstanceIdLabel.Text = _id;
		PageLifetimeTracker.Track(this, _id);

		DemoEntry.HandlerChanged += OnDemoEntryHandlerChanged;
		DemoEntry.HandlerChanging += OnDemoEntryHandlerChanging;
		DemoButton.HandlerChanged += OnDemoButtonHandlerChanged;

		Log("Constructor");
		Log($"Page Handler = {DescribeHandler(Handler)}");
		Log($"Entry Handler = {DescribeHandler(DemoEntry.Handler)}");
		UpdateNativeTypeLabel();
	}

	~LifecyclePage()
	{
		DemoLog.Write($"[{_id}] Finalizer");
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Log("OnAppearing");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		Log("OnDisappearing");

		if (CleanupStaticEventOnDisappearing)
		{
			// Appropriate in this demo because the page only needs the event while visible.
			UnsubscribeFromStaticEvent();
		}

		// Native subscriptions should usually be paired with handler disconnect/lifetime.
		// Async work should use cancellation.
		// IDisposable resources should be disposed when their owning scope ends.
		// Longer-lived view models may intentionally outlive a page.
		// Cleanup belongs where ownership ends.
	}

	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		Log($"HandlerChanging: {DescribeHandler(args.OldHandler)} -> {DescribeHandler(args.NewHandler)}");
		base.OnHandlerChanging(args);
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		Log("HandlerChanged");
		Log($"Handler = {Handler?.GetType().FullName ?? "NULL"}");
		Log($"PlatformView = {DescribePlatformView(Handler)}");
	}

	private void OnModifyNativeEntryClicked(object? sender, EventArgs e)
	{
		InspectAndModifyNativeEntry();
	}

	private void OnStaticEventToggled(object? sender, ToggledEventArgs e)
	{
		if (e.Value)
		{
			SubscribeToStaticEvent();
		}
		else
		{
			UnsubscribeFromStaticEvent();
		}
	}

	private void OnNativeEventToggled(object? sender, ToggledEventArgs e)
	{
		if (e.Value)
		{
			TrySubscribeToNativeEvent();
		}
		else
		{
			UnsubscribeFromNativeEvent();
		}
	}

	private void OnTimerToggled(object? sender, ToggledEventArgs e)
	{
		if (e.Value)
		{
			StartTimer();
		}
		else
		{
			StopTimer();
		}
	}

	private void SubscribeToStaticEvent()
	{
		if (_staticEventSubscribed)
		{
			return;
		}

		// INTENTIONAL DEMO LEAK.
		// The static event publisher outlives the page.
		// The delegate references this LifecyclePage instance.
		AppEvents.SomethingChanged += OnSomethingChanged;
		_staticEventSubscribed = true;
		Log("Subscribed to AppEvents");
	}

	private void UnsubscribeFromStaticEvent()
	{
		if (!_staticEventSubscribed)
		{
			return;
		}

		AppEvents.SomethingChanged -= OnSomethingChanged;
		_staticEventSubscribed = false;
		Log("Unsubscribed from AppEvents");
	}

	private void OnSomethingChanged(object? sender, EventArgs e)
	{
		Log("SomethingChanged received");
	}

	private void OnDemoEntryHandlerChanging(object? sender, HandlerChangingEventArgs e)
	{
		if (e.OldHandler is not null)
		{
			UnsubscribeFromNativeEvent();
		}
	}

	private void OnDemoEntryHandlerChanged(object? sender, EventArgs e)
	{
		Log($"Entry Handler = {DemoEntry.Handler?.GetType().FullName ?? "NULL"}");
		Log($"Entry PlatformView = {DescribePlatformView(DemoEntry.Handler)}");
		UpdateNativeTypeLabel();
		TrySubscribeToNativeEvent();
	}

	private void OnDemoButtonHandlerChanged(object? sender, EventArgs e)
	{
		Log($"Button Handler = {DemoButton.Handler?.GetType().FullName ?? "NULL"}");
		Log($"Button PlatformView = {DescribePlatformView(DemoButton.Handler)}");
	}

	private void TrySubscribeToNativeEvent()
	{
		if (!NativeEventSwitch.IsToggled || _nativeEventSubscribed)
		{
			return;
		}

#if ANDROID
		if (DemoEntry.Handler?.PlatformView is Android.Widget.EditText editText)
		{
			_androidFocusListener ??= new NativeFocusChangeListener(Log);
			editText.OnFocusChangeListener = _androidFocusListener;
			_nativeEventSubscribed = true;
			Log("Native focus event subscribed");
		}
#elif IOS
		if (DemoEntry.Handler?.PlatformView is UIKit.UITextField textField)
		{
			_iosEditingDidBeginHandler ??= OnIosEditingDidBegin;
			textField.EditingDidBegin += _iosEditingDidBeginHandler;
			_nativeEventSubscribed = true;
			Log("Native focus event subscribed");
		}
#else
		Log("Native event demo not implemented on this platform");
#endif
	}

	private void UnsubscribeFromNativeEvent()
	{
		if (!_nativeEventSubscribed)
		{
			return;
		}

#if ANDROID
		if (DemoEntry.Handler?.PlatformView is Android.Widget.EditText editText)
		{
			editText.OnFocusChangeListener = null;
		}
#elif IOS
		if (DemoEntry.Handler?.PlatformView is UIKit.UITextField textField && _iosEditingDidBeginHandler is not null)
		{
			textField.EditingDidBegin -= _iosEditingDidBeginHandler;
		}
#endif

		_nativeEventSubscribed = false;
		Log("Native focus event unsubscribed");
	}

#if IOS
	private void OnIosEditingDidBegin(object? sender, EventArgs e)
	{
		Log("Native focus event fired");
	}
#endif

	private void StartTimer()
	{
		if (_timer is not null)
		{
			return;
		}

		_timerTickCount = 0;
		_timer = Dispatcher.CreateTimer();
		_timer.Interval = TimeSpan.FromSeconds(1);
		_timer.Tick += OnTimerTick;
		_timer.Start();
		TimerStatusLabel.Text = "Timer running";
		Log("Timer started");
	}

	private void StopTimer()
	{
		if (_timer is null)
		{
			TimerStatusLabel.Text = "Timer stopped";
			return;
		}

		_timer.Stop();
		_timer.Tick -= OnTimerTick;
		_timer = null;
		TimerStatusLabel.Text = "Timer stopped";
		Log("Timer stopped");
	}

	private void OnTimerTick(object? sender, EventArgs e)
	{
		_timerTickCount++;
		TimerStatusLabel.Text = $"Timer ticks: {_timerTickCount}";
		Log("Timer tick");
	}

	private void InspectAndModifyNativeEntry()
	{
		UpdateNativeTypeLabel();

#if ANDROID
		if (DemoEntry.Handler?.PlatformView is Android.Widget.EditText editText)
		{
			editText.Hint = "Changed through Android native control";
			Log("Modified Android native entry hint");
			return;
		}
#elif IOS
		if (DemoEntry.Handler?.PlatformView is UIKit.UITextField textField)
		{
			textField.Placeholder = "Changed through iOS native control";
			Log("Modified iOS native entry placeholder");
			return;
		}
#endif

		Log("Native entry not available yet");
	}

	private void UpdateNativeTypeLabel()
	{
		NativeTypeLabel.Text = DemoEntry.Handler?.PlatformView?.GetType().FullName ?? "Waiting for handler...";
	}

	private void Log(string message)
	{
		DemoLog.Write($"[{_id}] {message}");
	}

	private static string DescribeHandler(IElementHandler? handler)
	{
		return handler?.GetType().Name ?? "NULL";
	}

	private static string DescribePlatformView(IElementHandler? handler)
	{
		return handler?.PlatformView?.GetType().FullName ?? "NULL";
	}

#if ANDROID
	private sealed class NativeFocusChangeListener(Action<string> log) : Java.Lang.Object, Android.Views.View.IOnFocusChangeListener
	{
		private readonly Action<string> _log = log;

		public void OnFocusChange(Android.Views.View? v, bool hasFocus)
		{
			_log($"Native focus event fired: hasFocus={hasFocus}");
		}
	}
#endif
}