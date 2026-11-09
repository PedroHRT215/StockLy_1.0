using Microsoft.Maui.Controls;
using Stockly.Models;
using Stockly.Services;
using Stockly.ViewModels;
using System;

namespace Stockly.Views
{
    public partial class AdminPage : ContentPage
    {
        public AdminPage()
        {
            InitializeComponent();
            var viewModel = new ProdutoViewModel();
            viewModel.ProdutoSelecionado = new Produto();
            BindingContext = viewModel;
        }


        private async void OnHistoricoSelecionado(object sender, SelectionChangedEventArgs e)
        {
            // Captura o item clicado
            var itemClicado = e.CurrentSelection.FirstOrDefault();

            // Se não houver nada selecionado (ocorre ao limpar a seleção), sai do método
            if (itemClicado == null) return;

            // Tenta converter o item para a sua classe Historico
            if (itemClicado is Historico historicoSelecionado)
            {
                var dbService = new DatabaseService();

                // Busca o usuário baseado no ID salvo no histórico
                Usuario usuarioDaReserva = dbService.GetUsuarioPorId(historicoSelecionado.UsuarioId);

                if (usuarioDaReserva != null)
                {
                    // Tenta navegar para a página de resumo
                    try
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new ResumoReservaPage(usuarioDaReserva));
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Erro de Navegação", $"Não foi possível abrir a tela: {ex.Message}", "OK");
                    }
                }
                else
                {
                    // Erro: O ID salvo no histórico não existe na tabela de Usuários
                    await DisplayAlert("Aviso", $"O Usuário (ID: {historicoSelecionado.UsuarioId}) não foi encontrado no banco de dados.", "OK");
                }
            }
            else
            {
                // Erro: A lista não é feita de objetos 'Historico'
                await DisplayAlert("Erro de Tipo", $"O item clicado não é um Histórico válido. Tipo recebido: {itemClicado.GetType().Name}", "OK");
            }

            // Limpa a seleção para permitir um novo clique no mesmo item
            ((CollectionView)sender).SelectedItem = null;
        }
    }

}