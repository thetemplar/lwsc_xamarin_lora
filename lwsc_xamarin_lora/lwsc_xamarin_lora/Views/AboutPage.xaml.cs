using lwsc_xamarin_lora.Services;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ShowInformation(object sender, EventArgs e)
        {
            App.ShowInformation = !App.ShowInformation;
            if (App.ShowInformation)
               btInformation.Text = "Hide Information";
            else
               btInformation.Text = "Show Information";
        }

        private void ShowIP(object sender, EventArgs e)
        {
            DependencyService.Get<IMessage>().ShortAlert("IP: " + App.IpAddress);
        }
    }
}