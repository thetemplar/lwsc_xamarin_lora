using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using lwsc_remote.Services;
using lwsc_remote.Views;

namespace lwsc_remote
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
