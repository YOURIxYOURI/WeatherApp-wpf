using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private const string ApiKey = "1d8d2d2b739044d39c7175543241502"; 
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CheckWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = CityTextBox.Text;
            if (!string.IsNullOrWhiteSpace(city))
            {
                await GetWeatherDataByCityAsync(city);
            }
            else
            {
                MessageBox.Show("Please enter a city name.");
            }
        }

        private async void CheckWeatherByCoordinatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(LatTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float latitude) &&
         float.TryParse(LonTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float longitude))
            {
                await GetWeatherDataByCoordinatesAsync(latitude, longitude);
            }
            else
            {
                MessageBox.Show("Invalid coordinates. Please enter valid numerical values.");
            }
        }

        private async Task GetWeatherDataByCityAsync(string city)
        {
            string apiUrl = $"http://api.weatherapi.com/v1/forecast.json?key={ApiKey}&q={city}&days=5&aqi=no&alerts=no";
            WeatherApiResponse weatherApiResponse = await GetWeatherDataAsync(apiUrl);
            DisplayWeatherData(weatherApiResponse);
        }

        private async Task GetWeatherDataByCoordinatesAsync(float latitude, float longitude)
        {
            string apiUrl = $"http://api.weatherapi.com/v1/forecast.json?key={ApiKey}&q={latitude},{longitude}&days=5&aqi=no&alerts=no";
            WeatherApiResponse weatherApiResponse = await GetWeatherDataAsync(apiUrl);
            DisplayWeatherData(weatherApiResponse);
        }

        private async Task<WeatherApiResponse> GetWeatherDataAsync(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                WeatherApiResponse weatherApiResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(responseBody);
                return weatherApiResponse;
            }
        }

        private void DisplayWeatherData(WeatherApiResponse weatherApiResponse)
        {
            // Current weather
            var currentWeather = weatherApiResponse.Current;
            TemperatureTextBlock.Text = $"Temperature: {currentWeather.TempC}°C";
            DescriptionTextBlock.Text = $"Description: {currentWeather.Condition.Text}";
            HumidityTextBlock.Text = $"Humidity: {currentWeather.Humidity}%";
            WindSpeedTextBlock.Text = $"Wind Speed: {currentWeather.WindKph} km/h";
            VisibilityTextBlock.Text = $"Visibility: {currentWeather.VisibilityKm} km";

            WeatherIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"http:{currentWeather.Condition.Icon}"));

            // Forecast for the next few days
            for (int i = 0; i < 3; i++)
            {
                var dailyForecast = weatherApiResponse.Forecast.ForecastDays[i];
                var textBlock = FindName($"Day{i + 1}TextBlock") as TextBlock;
                textBlock.Text = $"{dailyForecast.Date}: Max Temp: {dailyForecast.Day.MaxTempC}°C, Condition: {dailyForecast.Day.Condition.Text}";
            }

            var hourlyForecast = weatherApiResponse.Forecast.ForecastDays[0].Hourly;
            HourlyForecastPanel.Children.Clear();
            DateTime currentDateTime = DateTime.Parse(weatherApiResponse.Location.localtime);
            foreach (var hourForecast in hourlyForecast)
            {
                DateTime forecastDateTime = DateTime.Parse(hourForecast.Time);
                if (forecastDateTime >= currentDateTime)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = $"{forecastDateTime}: {hourForecast.TempC}°C, {hourForecast.Condition.Text}";
                    HourlyForecastPanel.Children.Add(textBlock);
                }
            }
        }
    }

    public class WeatherApiResponse
    {
        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("current")]
        public CurrentWeather Current { get; set; }

        [JsonProperty("forecast")]
        public Forecast Forecast { get; set; }
    }

    public class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("localtime")]
        public string localtime { get; set; }
    }

    public class CurrentWeather
    {
        [JsonProperty("temp_c")]
        public float TempC { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("wind_kph")]
        public float WindKph { get; set; }

        [JsonProperty("vis_km")]
        public float VisibilityKm { get; set; }
    }

    public class Forecast
    {
        [JsonProperty("forecastday")]
        public List<DailyForecast> ForecastDays { get; set; }
    }

    public class DailyForecast
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("hour")]
        public List<HourlyForecast> Hourly { get; set; }

        [JsonProperty("day")]
        public DayForecast Day { get; set; }
    }

    public class HourlyForecast
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("temp_c")]
        public float TempC { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }
    }

    public class DayForecast
    {
        [JsonProperty("maxtemp_c")]
        public float MaxTempC { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }
    }

    public class Condition
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon")]
        public string Icon {  get; set; }
    }
}
