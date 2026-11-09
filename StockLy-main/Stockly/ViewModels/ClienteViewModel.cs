using Stockly.Models;
using Stockly.Services;
using Stockly.Singleton;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Stockly.ViewModels
{
    public class ClienteViewModel : BaseNotifyViewModel
    {
        private DatabaseService _dbService;

        private ObservableCollection<Produto> _listaProdutos = new ObservableCollection<Produto>();
        public ObservableCollection<Produto> ListaProdutos
        {
            get { return _listaProdutos; }
            set
            {
                _listaProdutos = value;
                OnPropertyChanged();
            }
        }

        private Produto? _produtoSelecionado;
        public Produto? ProdutoSelecionado
        {
            get { return _produtoSelecionado; }
            set
            {
                _produtoSelecionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemProdutoSelecionado));
                QuantidadeSelecionadaTexto = "1";
            }
        }

        public bool TemProdutoSelecionado
        {
            get { return ProdutoSelecionado != null; }
        }

        private string _quantidadeSelecionadaTexto = "1";
        public string QuantidadeSelecionadaTexto
        {
            get { return _quantidadeSelecionadaTexto; }
            set
            {
                _quantidadeSelecionadaTexto = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Historico> _listaHistorico = new ObservableCollection<Historico>();
        public ObservableCollection<Historico> ListaHistorico
        {
            get { return _listaHistorico; }
            set
            {
                _listaHistorico = value;
                OnPropertyChanged();
            }
        }

        public string NomeUsuarioLogado
        {
            get { return UsuarioAtual.Instancia.UsuarioLogado?.Nome ?? "Cliente"; }
        }

        public Command SairCommand { get; }
        public Command SelecionarProdutoCommand { get; }
        public Command VerMeuHistoricoCommand { get; }

        public ClienteViewModel()
        {
            _dbService = new DatabaseService();

            SairCommand = new Command(() =>
            {
                UsuarioAtual.Instancia.Logout();
                Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
            });

            SelecionarProdutoCommand = new Command(ConfirmarReserva);
            VerMeuHistoricoCommand = new Command(CarregarHistorico);

            CarregarVitrine();
            CarregarHistorico();
        }

        // Carrega somente produtos que ainda possuem estoque.
        private void CarregarVitrine()
        {
            var produtosDoBanco = _dbService.ObterProdutos();
            var apenasComEstoque = produtosDoBanco
                .Where(produto => produto.Quantidade > 0)
                .ToList();

            ListaProdutos = new ObservableCollection<Produto>(apenasComEstoque);
        }

        // Busca diretamente o histórico do usuário autenticado.
        private void CarregarHistorico()
        {
            var usuario = UsuarioAtual.Instancia.UsuarioLogado;

            if (usuario == null)
            {
                ListaHistorico = new ObservableCollection<Historico>();
                return;
            }

            var historicoUsuario = _dbService.ObterHistoricoPorUsuario(usuario.Id);
            ListaHistorico = new ObservableCollection<Historico>(historicoUsuario);
        }

        // Executa a reserva e a baixa de estoque em uma única operação no SQLite.
        private async void ConfirmarReserva()
        {
            // Verifica o usuário logado
            Usuario usuarioLogado =
                UsuarioAtual.Instancia.UsuarioLogado;

            if (usuarioLogado == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    "Nenhum usuário está logado.",
                    "OK");

                return;
            }

            // Verifica se existe produto selecionado
            if (ProdutoSelecionado == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Aviso",
                    "Selecione um produto no catálogo primeiro.",
                    "OK");

                return;
            }

            // Valida a quantidade informada
            if (!int.TryParse(
                QuantidadeSelecionadaTexto,
                out int qtdDesejada) ||
                qtdDesejada <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    "Insira uma quantidade válida maior que zero.",
                    "OK");

                return;
            }

            // Verifica o estoque apresentado na tela
            if (qtdDesejada > ProdutoSelecionado.Quantidade)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Estoque Insuficiente",
                    $"O produto possui apenas " +
                    $"{ProdutoSelecionado.Quantidade} unidades disponíveis.",
                    "OK");

                return;
            }

            // Solicita confirmação
            bool confirma =
                await Application.Current.MainPage.DisplayAlert(
                    "Confirmar",
                    $"Deseja reservar {qtdDesejada}x do item " +
                    $"{ProdutoSelecionado.Nome}?",
                    "Sim",
                    "Não");

            if (!confirma)
            {
                return;
            }

            // Atualiza o estoque e registra o histórico
            // dentro da mesma transação SQLite.
            bool sucesso = _dbService.RegistrarReserva(
                usuarioLogado,
                ProdutoSelecionado.Id,
                qtdDesejada);

            if (!sucesso)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Reserva não realizada",
                    "O produto pode ter sido atualizado por outro usuário " +
                    "ou não possui mais estoque suficiente. Atualize a vitrine.",
                    "OK");

                CarregarVitrine();

                return;
            }

            await Application.Current.MainPage.DisplayAlert(
                "Sucesso",
                "Reserva registrada com sucesso!",
                "OK");

            // Limpa a seleção
            ProdutoSelecionado = null;

            QuantidadeSelecionadaTexto = "1";

            // Atualiza as informações da tela
            CarregarVitrine();

            CarregarHistorico();
        }
    }
}