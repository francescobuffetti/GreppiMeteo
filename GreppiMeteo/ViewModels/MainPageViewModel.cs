using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GreppiMeteo.Models;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreppiMeteo.ViewModels
{
    public partial class MainPageViewModel:ObservableObject
    {
        DateTime data = DateTime.Today;

        [ObservableProperty]
        private string locality;
        
        [ObservableProperty] 
        private string country;

        [ObservableProperty]
        private string currentDescription;

        [ObservableProperty]
        private double currentTemp;

        [ObservableProperty]
        private OneCallApi meteo;

        [ObservableProperty]
        private List<Hourly> hourlies;

        [ObservableProperty]
        private Hourly myHourly;

        [ObservableProperty]
        private string unit;

        public Page MainPage => Application.Current.MainPage;

        public string Data { get
            {
                data.AddDays(1);
                return data.ToShortDateString();
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Task.Delay(300).ConfigureAwait(true);
            await MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        private void SelectHour(Hourly hourly)
        {
            if (hourly is not null)
            {
                MyHourly = hourly;
            }
        }

    }
}
