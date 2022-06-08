using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Weather
{
    public partial class MainWindow : Window
    {

        WebClient client = new WebClient();

        public string m_WeatherUrl = "https://api.openweathermap.org/data/2.5/weather?q=";
        public string m_ForecastWeather = "https://api.openweathermap.org/data/2.5/forecast?q=";
        public string m_WeatherKey = "&appid=38f5beab20673e372f5aacfb5e63f5f4";
        public string m_CityName;
        string m_JsonDataUrl;
        JObject m_JsonData;

        public MainWindow()
        {
            InitializeComponent();
            GetDateTime(DateTime.Now);

            //The default city
            FindThisCity("Paris");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            m_CityName = cityTxt.Text;
            FindThisCity(m_CityName);
        }

        public void FindThisCity(string city)
        {
            try
            {
                m_JsonDataUrl = new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(m_ForecastWeather + city + m_WeatherKey + "&lang=fr&units=metric");
                m_JsonData = (JObject)JsonConvert.DeserializeObject(m_JsonDataUrl);
            }
            catch
            {
                MessageBox.Show("Erreur : Cette ville n'existe pas. Veuillez réessayer", "Crédits", MessageBoxButton.OK, MessageBoxImage.Information);
                cityTxt.Text = "";
            }

            // Display the informations of the current day
            cityTitle.Content = m_JsonData["city"]["name"];

            temp.Content = NumberWithoutComma("+" + m_JsonData["list"][0]["main"]["temp"]) + "°C";
            wind.Content = NumberWithoutComma(m_JsonData["list"][0]["wind"]["speed"].ToString()) + "km/h";
            desc.Content = CapitalizeStr(m_JsonData["list"][0]["weather"][0]["description"].ToString());

            ForeCastWeather(m_JsonData);
            SetHeaderImage();
        }

        public void ForeCastWeather(JObject jsonData)
        {
            // Number of days in json data
            var dtNow = int.Parse(jsonData["cnt"].ToString());

            // Number of days to choose
            int nbForeCastDays = 0;

            // Day in seconds
            int twentyForHoursInSeconds = 86400;

            // Current day in seconds
            var CurrentDayInSeconds = int.Parse(jsonData["list"][0]["dt"].ToString());

            // Get the three next days
            for (int i = 0; i < dtNow; i++)
            {
                var nextDayInSeconds = int.Parse(jsonData["list"][i]["dt"].ToString()); 
                int resultSecBetweenDays = nextDayInSeconds - CurrentDayInSeconds;

                // Get only the next 3 days from five-day forecast list including several updates every 3 hours

                if (nbForeCastDays <= 2 && resultSecBetweenDays == twentyForHoursInSeconds)
                {
                    if (nbForeCastDays == 0)
                    {
                        nbForeCastDays++;
                        fc1temp.Content = NumberWithoutComma("+" + jsonData["list"][i]["main"]["temp"]) + "°C";
                        CurrentDayInSeconds = int.Parse(jsonData["list"][i]["dt"].ToString());
                        SetForeCastImage(i, nbForeCastDays);
                        PrevInfos(i, nbForeCastDays, jsonData, new List<Label> { datePrev1, descPrev1, windPrev1});

                    }
                    else if (nbForeCastDays == 1)
                    {
                        nbForeCastDays++;
                        fc2temp.Content = NumberWithoutComma("+" + jsonData["list"][i]["main"]["temp"]) + "°C";
                        CurrentDayInSeconds = int.Parse(jsonData["list"][i]["dt"].ToString());
                        SetForeCastImage(i, nbForeCastDays);
                        PrevInfos(i, nbForeCastDays, jsonData, new List<Label> { datePrev2, descPrev2, windPrev2 });
                    }
                    else if (nbForeCastDays == 2)
                    {
                        nbForeCastDays++;
                        fc3temp.Content = NumberWithoutComma("+" + jsonData["list"][i]["main"]["temp"]) + "°C";
                        CurrentDayInSeconds = int.Parse(jsonData["list"][i]["dt"].ToString());
                        SetForeCastImage(i, nbForeCastDays);
                        PrevInfos(i, nbForeCastDays, jsonData, new List<Label> { datePrev3, descPrev3, windPrev3 });
                    }
                }
            }
        }

        // Display some informations on the 3 forecast days
        public void PrevInfos(int i, int date, JObject jsonData, List<Label> prev)
        {
            prev[0].Content = CapitalizeStr(DateTime.Now.AddDays(date).ToString("dddd dd MMMM yyyy"));
            prev[1].Content = CapitalizeStr(jsonData["list"][i]["weather"][0]["description"].ToString());
            prev[2].Content = NumberWithoutComma("Vent : " + jsonData["list"][i]["wind"]["speed"]) + " km/h";
        }

        public string NumberWithoutComma(string number)
        {
            string[] splitNumber = number.Split(',');

            return splitNumber[0];
        }

        // Get the current date
        public void GetDateTime(DateTime date)
        {
            dateTxt.Content = CapitalizeStr(date.ToString("dddd dd MMMM yyyy"));

            foreCast1.Content = CapitalizeStr(date.AddDays(1).ToString("ddd"));
            foreCast2.Content = CapitalizeStr(date.AddDays(2).ToString("ddd"));
            foreCast3.Content = CapitalizeStr(date.AddDays(3).ToString("ddd"));
        }

        public string CapitalizeStr(string str)
        {
            return str[0].ToString().ToUpper() + str.Substring(1);
        }

        // Set the correct pictures of weather for the header
        public void SetHeaderImage()
        {
            string meteo = m_JsonData["list"][0]["weather"][0]["main"].ToString();

            SetWeatherImage(meteo, headerImg, new List<string> { "sun.jpg", "nuage.jpg", "rain.jpg" });
        }

        // Set the correct pictures of weather for the forecast days
        public void SetForeCastImage(int i, int nbForeCastDays)
        {
            List<Image> foreCastImg = new List<Image>() { foreCast1Img, foreCast2Img, foreCast3Img };
            string meteo = m_JsonData["list"][i]["weather"][0]["main"].ToString();

            SetWeatherImage(meteo, foreCastImg[nbForeCastDays-1], new List<string> { "sun-ico.png", "nuage-ico.png", "rain-ico.png"});
        }


        // Set the correct pictures depending on the weather
        public void SetWeatherImage(string meteo, Image imgSrc, List<string> images)
        {
            if (meteo == "Clear")
            {
                imgSrc.Source = new BitmapImage(new Uri(images[0], UriKind.Relative));
            }
            if (meteo == "Clouds")
            {
                imgSrc.Source = new BitmapImage(new Uri(images[1], UriKind.Relative));
            }
            if (meteo == "Rain")
            {
                imgSrc.Source = new BitmapImage(new Uri(images[2], UriKind.Relative));
            }
        }

        private void btnInfos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Développé par : Kévin ROBIN", "Crédits", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
