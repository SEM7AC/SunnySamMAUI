using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  // For asynchronous tasks
using Microsoft.Maui.Controls;              // For UI components like ContentPage
using Microsoft.Maui.Devices.Sensors;       // For Geolocation API
using System.Net.Http;                      // For HttpClient to handle web requests
using Newtonsoft.Json;                      // For JSON serialization and deserialization


// string APIKey = "5768652e52bd24c1d17bdf661a72eb07";
namespace SunnySamMAUI
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Fetch location-based weather
            var location = await GetCurrentLocationAsync();
            if (location != null)
            {
                string cityName = await GetCityNameAsync(location.Latitude, location.Longitude);
                CityEntry.Text = cityName; // Update the left column with current location city
                await GetWeather(cityName); // Fetch geolocation-based weather
            }

            // Fetch default weather for Savannah in the right column
            UserCityEntry.Text = "Savannah"; // Pre-fill the user city Entry
            await GetWeatherForUserCity("Savannah"); // Fetch and display weather for Savannah
        }

        async Task PlayWeatherTTS(double temperature)
        {
            string message = "";

            if (temperature < 32)
            {
                message = "It's freezing out there! You'd better bundle up.";
            }
            else if (temperature >= 32 && temperature < 50)
            {
                message = "It's pretty chilly, wear something warm!";
            }
            else if (temperature >= 50 && temperature < 75)
            {
                message = "What a beautiful day—enjoy the perfect weather!";
            }
            else if (temperature >= 75 && temperature < 90)
            {
                message = "It's getting warm out there. Stay hydrated!";
            }
            else
            {
                message = "Whoa! It's scorching hot. Find some shade!";
            }

            // Use TTS to speak the message
            await TextToSpeech.Default.SpeakAsync(message);
        }




        async Task GetWeather(string cityName)
        {
            try
            {
                string apiKey = "5768652e52bd24c1d17bdf661a72eb07";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=imperial";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var weatherInfo = JsonConvert.DeserializeObject<Root>(json);

                        double temperature = weatherInfo.main.temp ?? 0; // Default to 0 if null


                        // Display weather data
                        WeatherIcon.Source = "https://openweathermap.org/img/w/" + weatherInfo.weather[0].icon + ".png";
                        WeatherCondition.Text = weatherInfo.weather[0].description;
                        Temperature.Text = $"{temperature} °F";
                        WindDetails.Text = $"Wind: {weatherInfo.wind.speed} mph";

                        // Play TTS message after displaying weather
                        await PlayWeatherTTS(temperature);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Unable to fetch weather data. Please try again.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetWeather: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while fetching weather data.", "OK");
            }
        }






        async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
                {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
                });

                if (location != null)
                {
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                return location;
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location: {ex.Message}");
        }
        return null;
        }

        async Task<string> GetCityNameAsync(double lat, double lon)
        {
            string apiKey = "5768652e52bd24c1d17bdf661a72eb07";
            string url = $"https://api.openweathermap.org/geo/1.0/reverse?lat={lat}&lon={lon}&limit=1&appid={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var locationData = JsonConvert.DeserializeObject<List<GeoLocation>>(response);
                return locationData?.FirstOrDefault()?.name ?? "Unknown City";
            }
        }


        // A class for deserializing the reverse geocoding response
        public class GeoLocation
        {
            public string name { get; set; }
            public string country { get; set; }
        }

        private async void OnRefreshWeatherClicked(object sender, EventArgs e)
        {
            var location = await GetCurrentLocationAsync();
            if (location != null)
            {
                string cityName = await GetCityNameAsync(location.Latitude, location.Longitude);

                CityEntry.Text = cityName; // Optional: update the UI with city name
                await GetWeather(cityName); // Automatically fetch and display the weather
            }
            else
            {
                await DisplayAlert("Error", "Unable to fetch location", "OK");
            }
        }

        private async void OnUserCityWeatherClicked(object sender, EventArgs e)
        {
            string userCityName = UserCityEntry.Text; // Retrieve text from the new Entry

            if (!string.IsNullOrWhiteSpace(userCityName))
            {
                try
                {
                    string apiKey = "5768652e52bd24c1d17bdf661a72eb07";
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={userCityName}&appid={apiKey}&units=imperial";

                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(url);
                        var weatherInfo = JsonConvert.DeserializeObject<Root>(response);

                        UserWeatherIcon.Source = "https://openweathermap.org/img/w/" + weatherInfo.weather[0].icon + ".png";
                        UserWeatherCondition.Text = weatherInfo.weather[0].description;
                        UserTemperature.Text = $"{weatherInfo.main.temp} °F";
                        UserWindDetails.Text = $"Wind: {weatherInfo.wind.speed} mph";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching weather for {userCityName}: {ex.Message}");
                    await DisplayAlert("Error", $"Unable to fetch weather for {userCityName}.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid city name.", "OK");
            }
        }

        async Task GetWeatherForUserCity(string cityName)
        {
            try
            {
                string apiKey = "5768652e52bd24c1d17bdf661a72eb07";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=imperial";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var weatherInfo = JsonConvert.DeserializeObject<Root>(response);

                    UserWeatherIcon.Source = "https://openweathermap.org/img/w/" + weatherInfo.weather[0].icon + ".png";
                    UserWeatherCondition.Text = weatherInfo.weather[0].description;
                    UserTemperature.Text = $"{weatherInfo.main.temp} °F";
                    UserWindDetails.Text = $"Wind: {weatherInfo.wind.speed} mph";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather for {cityName}: {ex.Message}");
                await DisplayAlert("Error", $"Unable to fetch weather for {cityName}.", "OK");
            }
        }




    }

}
