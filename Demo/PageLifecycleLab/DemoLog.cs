using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PageLifecycleLab;

public static class DemoLog
{
	private const int MaxLines = 100;
	private static readonly ObservableCollection<string> _lines = [];

	public static event EventHandler<string>? MessageLogged;

	public static ReadOnlyObservableCollection<string> Lines { get; } = new(_lines);

	public static void Write(string message)
	{
		Debug.WriteLine(message);

		MainThread.BeginInvokeOnMainThread(() =>
		{
			_lines.Add(message);
			while (_lines.Count > MaxLines)
			{
				_lines.RemoveAt(0);
			}

			MessageLogged?.Invoke(null, message);
		});
	}

	public static void Clear()
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			_lines.Clear();
			_lines.Add("[Main] Log cleared");
			MessageLogged?.Invoke(null, "[Main] Log cleared");
		});
	}
}