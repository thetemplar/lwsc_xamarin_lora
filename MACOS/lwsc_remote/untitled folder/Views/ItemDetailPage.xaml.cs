using System.ComponentModel;
using Xamarin.Forms;
using lwsc_remote.ViewModels;

namespace lwsc_remote.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
