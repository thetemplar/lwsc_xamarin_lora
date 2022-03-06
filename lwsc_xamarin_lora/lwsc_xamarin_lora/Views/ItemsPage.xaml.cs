using lwsc_xamarin_lora.Models;
using lwsc_xamarin_lora.Services;
using lwsc_xamarin_lora.ViewModels;
using lwsc_xamarin_lora.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace lwsc_xamarin_lora.Views
{
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel _viewModel;
        ItemsViewModel _viewModelSelected;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ItemsViewModel();
            _viewModelSelected = new ItemsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        
        private void functionList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Machine item = e.Item as Machine;
            _viewModelSelected.Items.Add(item);

            if(RESTful.Fire(item))
            {
                this.functionList.SelectedItem = null;
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if(Mode.Text == "Selektierte")
            {
                BindingContext = _viewModelSelected;
                Mode.Text = "Alle zeigen";
            } else
            {
                Mode.Text = "Selektierte";
                BindingContext = _viewModel;

            }
        }

        private void ToolbarItem_Clear(object sender, EventArgs e)
        {
            Mode.Text = "Selektierte";
            BindingContext = _viewModel;
            _viewModelSelected = new ItemsViewModel();
        }
    }
}