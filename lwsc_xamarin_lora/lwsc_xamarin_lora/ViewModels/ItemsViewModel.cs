﻿using lwsc_xamarin_lora.Models;
using lwsc_xamarin_lora.Services;
using lwsc_xamarin_lora.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace lwsc_xamarin_lora.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Machine _selectedItem;

        public ObservableCollection<Machine> Items { get; private set; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<Machine> ItemTapped { get; }

        public ItemsViewModel()
        {
            Title = "Auslöser";
            Items = new ObservableCollection<Machine>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Machine>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
        }
        public ItemsViewModel(IEnumerable<Machine> clone)
        {
            Title = "Auslöser";
            Items = new ObservableCollection<Machine>(clone);
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Machine>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                Items = new ObservableCollection<Machine>(Items.OrderBy(x => x.Name));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Machine SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        void OnItemSelected(Machine item)
        {
            if (item == null)
                return;
               //return false;

            
        }
    }
}