namespace PageLifecycleLab;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(LifecyclePage), typeof(LifecyclePage));
	}
}
