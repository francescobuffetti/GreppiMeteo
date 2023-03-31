using GreppiMeteo.ViewModels;
using GreppiMeteo.Texts;

namespace GreppiMeteo.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
        
	}



    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set("geolocation", e.Value);
    }

    protected override void OnAppearing()
    {
        localita.Text = Preferences.Default.Get("localitaPredefinita", "Monza");
        geolocationSwitch.IsToggled = Preferences.Default.Get("geolocation", true);
    }

    private async void localita_SearchButtonPressed(object sender, EventArgs e)
    {
        string address = localita.Text;
        IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);

        Location location = locations?.FirstOrDefault();

        if (location == null)
            await DisplayAlert($"{TextsResource.Error1}", $"{TextsResource.LocNotFound}", "Ok");
        else
        {
            Preferences.Default.Set("lat", location.Latitude);
            Preferences.Default.Set("lon", location.Longitude);
            Preferences.Default.Set("localitaPredefinita", localita.Text);
            await DisplayAlert($"{TextsResource.Success1}", $"{TextsResource.LocSaved}", "Ok");
            model.Locality = address;
        }
    }

    bool isExpanded = false;
    void ViewLanguagesTap(object sender, EventArgs args)
    {
        isExpanded = !isExpanded;

        layoutLanguage.IsVisible = isExpanded;

        lblMore.Text = isExpanded ? $"{TextsResource.ReadLess}" : $"{TextsResource.ViewLang}";

    }
}