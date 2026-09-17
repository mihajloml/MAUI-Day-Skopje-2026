namespace PageLifecycleLab;

public static class PageLifetimeTracker
{
	private static WeakReference<LifecyclePage>? _lastPage;

	public static string? LastTrackedId { get; private set; }
	public static string? LastTrackedPageName { get; private set; }

	// WeakReference lets us observe whether the page is alive without becoming an owner of the page.
	public static void Track(LifecyclePage page, string id)
	{
		_lastPage = new WeakReference<LifecyclePage>(page);
		LastTrackedId = id;
		LastTrackedPageName = nameof(LifecyclePage);
	}

	public static bool IsAlive => _lastPage?.TryGetTarget(out _) == true;
}