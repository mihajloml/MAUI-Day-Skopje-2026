namespace PageLifecycleLab;

public static class AppEvents
{
	private static EventHandler? _somethingChanged;

	public static event EventHandler? SomethingChanged
	{
		add => _somethingChanged += value;
		remove => _somethingChanged -= value;
	}

	public static int SubscriberCount => _somethingChanged?.GetInvocationList().Length ?? 0;

	public static void RaiseSomethingChanged()
	{
		_somethingChanged?.Invoke(null, EventArgs.Empty);
	}
}