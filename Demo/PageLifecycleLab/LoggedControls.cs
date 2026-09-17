using System.Threading;

namespace PageLifecycleLab;

internal static class ConstructionScope
{
	private static readonly AsyncLocal<string?> CurrentPageIdValue = new();

	public static string? CurrentPageId
	{
		get => CurrentPageIdValue.Value;
		set => CurrentPageIdValue.Value = value;
	}
}

public class LoggedLabel : Label
{
	public LoggedLabel()
	{
		LogConstruction(nameof(LoggedLabel));
	}

	private static void LogConstruction(string controlName)
	{
		if (ConstructionScope.CurrentPageId is { } id)
		{
			DemoLog.Write($"[{id}] Managed control created: {controlName}");
		}
	}
}

public class LoggedEntry : Entry
{
	public LoggedEntry()
	{
		if (ConstructionScope.CurrentPageId is { } id)
		{
			DemoLog.Write($"[{id}] Managed control created: {nameof(LoggedEntry)}");
		}
	}
}

public class LoggedButton : Button
{
	public LoggedButton()
	{
		if (ConstructionScope.CurrentPageId is { } id)
		{
			DemoLog.Write($"[{id}] Managed control created: {nameof(LoggedButton)}");
		}
	}
}