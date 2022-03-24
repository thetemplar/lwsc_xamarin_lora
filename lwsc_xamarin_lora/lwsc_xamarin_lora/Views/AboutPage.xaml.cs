using lwsc_xamarin_lora.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    class CheckUser
    {
        public string result;
        public string rights;
    }
    
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            Task.Run(
                async () => {
                    var r = await RESTful.IsInGPSRangeAsyncTimeout(3000);
                    if (r == RESTful.GPSStatus.NOTAVAILIBLE)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht verfügbar!");
                    if (r == RESTful.GPSStatus.OUTOFRANGE_NEAR)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht auf dem Gelände! (<5km)");
                    if (r == RESTful.GPSStatus.OUTOFRANGE_FAR)
                        DependencyService.Get<IMessage>().ShortAlert("GPS: Nicht auf dem Gelände! (>5km)");
                }).Wait();
        }

        private void ShowInformation(object sender, EventArgs e)
        {
            App.ShowInformation = !App.ShowInformation;
            if (App.ShowInformation)
            {
                btInformation.Text = "Auslöse-Information ausblenden";
                DependencyService.Get<IMessage>().ShortAlert("Ab jetzt werden Auslöse-Infos ausgeblendet.");
            }
            else
            {
                btInformation.Text = "Auslöse-Informationen anzeigen";
                DependencyService.Get<IMessage>().ShortAlert("Ab jetzt werden Auslöse-Infos gezeigt.");
            }
        }

        private void ShowIP(object sender, EventArgs e)
        {
            if (App.RemoteEP.Address.ToString().Length > 0)
                DependencyService.Get<IMessage>().ShortAlert("IP: " + App.RemoteEP.Address.ToString());
            else
                DependencyService.Get<IMessage>().ShortAlert("IP: <Internet>");
        }

        private void EnableExperimental(object sender, EventArgs e)
        {
            App.Experimental = !App.Experimental;
            if (App.Experimental)
            {
                btExperimental.Text = "Experimental deaktivieren";
                DependencyService.Get<IMessage>().ShortAlert("Experimental: Aktiviert (schneller)");
            }
            else
            {
                btExperimental.Text = "Experimental aktivieren";
                DependencyService.Get<IMessage>().ShortAlert("Experimental: Deaktiviert (sicherer)");
            }
        }

        private void TestGPS(object sender, EventArgs e)
        {
            RESTful.DoGPSCheck(true, true);
        }

        private void Login(object sender, EventArgs e)
        {
            var status = RESTful.Query("/check_user?username=" + eUsername.Text + "&password=" + ePassword.Text + "", RESTful.RESTType.GET, out string res);

            var parsedJson = JsonConvert.DeserializeObject<CheckUser>(res);

            if (parsedJson.result == "no auth")
                DependencyService.Get<IMessage>().ShortAlert("User not found.");
            else if (parsedJson.result == "wrong password")
                DependencyService.Get<IMessage>().ShortAlert("Wrong Password.");
            else if (parsedJson.result == "success")
            {
                App.Username = eUsername.Text;
                App.Password = ePassword.Text;
                string r = "";
                switch (parsedJson.rights)
                {
                    case "0": r = "Unknown"; break;
                    case "1": r = "User"; break;
                    case "2": r = "Manager"; break;
                    case "3": r = "Admin"; break;
                    default: r = "?!"; break;
                }
                DependencyService.Get<IMessage>().ShortAlert("Hello " + App.Username + "! (" + r + ")");
            }
            else
                DependencyService.Get<IMessage>().ShortAlert(res);

        }
    }
}