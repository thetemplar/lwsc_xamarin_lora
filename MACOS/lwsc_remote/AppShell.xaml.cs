using lwsc_remote.ViewModels;
using lwsc_remote.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace lwsc_remote
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
