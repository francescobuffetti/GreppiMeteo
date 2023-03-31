using GreppiMeteo.Views;
using System.Globalization;

namespace GreppiMeteo;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new MainPage());
	}

    protected override void OnStart()
    {
        base.OnStart();

        SetCulture(CultureInfo.CurrentCulture);
    }

    private void SetCulture(CultureInfo currentCulture)
    {
        CultureInfo.CurrentCulture = currentCulture;
        CultureInfo.CurrentUICulture = currentCulture;
        CultureInfo.DefaultThreadCurrentCulture = currentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
    }
}
