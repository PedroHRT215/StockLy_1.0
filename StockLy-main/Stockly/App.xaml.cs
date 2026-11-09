using Microsoft.Extensions.DependencyInjection;
using Stockly.Views;

namespace Stockly
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new LoginPage());

            //window.Height = 750; //Altura
            //window.Width = 500;  //Largura

            return window;
        }

    }
}