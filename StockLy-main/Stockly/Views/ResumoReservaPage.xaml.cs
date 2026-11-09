using Stockly.Models;
using Stockly.ViewModels;
using Microsoft.Maui.Controls;

namespace Stockly.Views
{
    public partial class ResumoReservaPage : ContentPage
    {
        public ResumoReservaPage(Usuario usuario)
        {
            InitializeComponent();
            BindingContext = new VisualizarReservaAdminViewModel(usuario);
        }
    }
}