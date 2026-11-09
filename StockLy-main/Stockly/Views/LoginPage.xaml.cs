using Stockly.ViewModels;

namespace Stockly.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        BindingContext = new UsuarioViewModel();
    }
}