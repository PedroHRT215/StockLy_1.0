using Microsoft.UI.Xaml;
using Stockly.Models;
using Stockly.Services;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Stockly.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            DatabaseService banco = new DatabaseService();

            bool criado = banco.CriarAdministradorInicial(
                new Usuario
                {
                    Nome = "Administrador StockLy",
                    CPF = "00000000000",
                    Email = "admin@stockly.com",
                    Senha = "123456789"
                });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
