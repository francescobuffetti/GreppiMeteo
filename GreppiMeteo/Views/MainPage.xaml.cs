using CommunityToolkit.Mvvm.Input;
using GreppiMeteo.Models;
using GreppiMeteo.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http.Json;
using GreppiMeteo.Texts;
using GreppiMeteo.Constants;

namespace GreppiMeteo.Views;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Hourly> Hourlies;
    public ObservableCollection<Daily> Daily;
    public Hourly MyHourly;

	public MainPage()
	{
		InitializeComponent();
	}

    private async Task<string> GetGeocodeReverseData(double latitude, double longitude)
    {
        IEnumerable<Placemark> placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

        Placemark placemark = placemarks?.FirstOrDefault();

        if (placemark != null)
        {
            return
                placemark.Locality;
        }

        return "";
    }

    private async Task<string> GetCountry(double latitude, double longitude)
    {
        IEnumerable<Placemark> placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);

        Placemark placemark = placemarks?.FirstOrDefault();

        if (placemark != null)
        {
            return
                placemark.CountryName;
        }

        return "";
    }

    public async Task<Location> GetCurrentLocation() 
    {
        try
        {

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));


            Location location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
                return location;
        }
        catch (Exception ex)
        {
            
        }
        Location defaultLocation = new Location() { Latitude = 45.464664, Longitude = 9.188540 };
        return defaultLocation;
    }

    protected override async void OnAppearing()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.openweathermap.org/");
        var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        Hourlies = new ObservableCollection<Hourly>();
        Daily = new ObservableCollection<Daily>();
        
        if (Preferences.Default.Get("geolocation",true))
        {
            Location location = await GetCurrentLocation();
            string locality = await GetGeocodeReverseData(location.Latitude, location.Longitude);
            string country = await GetCountry(location.Latitude, location.Longitude);
            var response = await client.GetFromJsonAsync<OneCallApi>($"data/2.5/onecall?lat={location.Latitude}&lon={location.Longitude}&lang={lang}&units=metric&exclude=minutely,alerts&appid={ApiConstant.OpenWeatherMapKey}");
            HourlyAndDaily(response, locality, country);
            
        }
        else
        {
            double lat = Preferences.Default.Get("lat", 45.464664);
            double lon = Preferences.Default.Get("lon", 9.188540);
            string locality = await GetGeocodeReverseData(lat, lon);
            string country = await GetCountry(lat, lon);
            var response = await client.GetFromJsonAsync<OneCallApi>($"data/2.5/onecall?lat={lat}&lon={lon}&lang={lang}&units=metric&exclude=minutely,alerts&appid={ApiConstant.OpenWeatherMapKey}");
            HourlyAndDaily(response, locality, country);
            
            
        }
    }

    public void HourlyAndDaily(OneCallApi response, string locality = "default", string country = "def")
    {
        if (Hourlies.Count >= 1)
        {
            Hourlies.Clear();
        }

        foreach (var item in response.Hourly)
        {
            Hourlies.Add(item);
        }


        if (Daily.Count >= 1)
        {
            Daily.Clear();
        }

        foreach (var item in response.Daily)
        {
            Daily.Add(item);
        }

        model.MyHourly = Hourlies.ElementAt(0);
        cvDaily.ItemsSource = Daily;
        cvHourly.ItemsSource = Hourlies;
        if(locality != "default")
        model.Locality = locality;
        model.Country = country;
        model.Meteo = response;
        model.CurrentDescription = response.Current.Weather[0].Description;
        model.CurrentTemp = response.Current.Temp;
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }



    private async void localita_SearchButtonPressed(object sender, EventArgs e)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.openweathermap.org/");
        string address = localita.Text;
        IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);
        var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;


        Location location = locations?.FirstOrDefault();

        if (location == null)
            await DisplayAlert($"{TextsResource.Error1}", $"{TextsResource.LocNotFound}", "Ok");
        else
        {
            model.Locality = address;
            var response = await client.GetFromJsonAsync<OneCallApi>($"data/2.5/onecall?lat={location.Latitude}&lon={location.Longitude}&lang={lang}&units=metric&exclude=minutely,alerts&appid={ApiConstant.OpenWeatherMapKey}");
            string country = await GetCountry(location.Latitude, location.Longitude);
            HourlyAndDaily(response, country: country);
        }
    }

    private static void ChangeSurfaceControls(VisualElement child, bool isSelected)
    {
        Label labelTemperature = child.FindByName<Label>("labelTemperature");
        Image imageSmallWeather = child.FindByName<Image>("imageSmallWeather");
        Label labelHour = child.FindByName<Label>("labelHour");
        Label labelDay = child.FindByName<Label>("labelDay");

        string visualStateControl = isSelected ? "Selected" : "Normal";
        VisualStateManager.GoToState(labelTemperature, visualStateControl);
        VisualStateManager.GoToState(imageSmallWeather, visualStateControl);
        VisualStateManager.GoToState(labelHour, visualStateControl);
        VisualStateManager.GoToState(labelDay, visualStateControl);
    }

    private Border _selectedSurfaceWeather = null;

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Border surfaceWeather = sender as Border;
        if (_selectedSurfaceWeather != null)
        {
            VisualStateManager.GoToState(_selectedSurfaceWeather, "Normal");
            ChangeSurfaceControls(_selectedSurfaceWeather, false);
        }
        if (_selectedSurfaceWeather != surfaceWeather)
        {
            _selectedSurfaceWeather = surfaceWeather;
            VisualStateManager.GoToState(_selectedSurfaceWeather, "Selected");
            ChangeSurfaceControls(_selectedSurfaceWeather, true);
        }
    }

    bool isExpanded = false;

    void OnReadMoreTapped(object sender, EventArgs args)
    {
        isExpanded = !isExpanded;

        // cambia la visibilità dei border in base allo stato dell'espansione
        gridAdditionalDescriptionClimate.IsVisible = isExpanded;

        // cambia il testo della label in base allo stato dell'espansione
        ((Label)sender).Text = isExpanded ? $"{TextsResource.ReadLess}" : $"{TextsResource.ReadMore}";
    }

}

