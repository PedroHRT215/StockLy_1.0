using Microsoft.Maui.Controls;
using Stockly.ViewModels;

namespace Stockly.Views
{
    public partial class RegistroPage : ContentPage
    {
        public RegistroPage()
        {
            InitializeComponent();
            BindingContext = new UsuarioViewModel();
        }
    }
}