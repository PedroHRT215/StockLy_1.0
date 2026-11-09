using Microsoft.Maui.Controls;
using Stockly.ViewModels;

namespace Stockly.Views
{
    public partial class ClientePage : ContentPage
    {
        public ClientePage()
        {
            InitializeComponent();
            BindingContext = new ClienteViewModel();
        }
    }
}