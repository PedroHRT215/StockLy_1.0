using Stockly.Models;
using Stockly.Services;
using Stockly.Singleton;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace Stockly.ViewModels
{
    public class ProdutoViewModel : BaseNotifyViewModel
    {
        private DatabaseService _dbService;

        public string NomeUsuarioLogado
        {
            get { return UsuarioAtual.Instancia.UsuarioLogado?.Nome ?? "Administrador"; }
        }

        private List<Produto> _listaProdutos = new List<Produto>();
        public List<Produto> ListaProdutos
        {
            get { return _listaProdutos; }
            set
            {
                _listaProdutos = value;
                OnPropertyChanged();
            }
        }

        private List<Historico> _listaHistorico = new List<Historico>();
        public List<Historico> ListaHistorico
        {
            get { return _listaHistorico; }
            set
            {
                _listaHistorico = value;
                OnPropertyChanged();
            }
        }

        private Produto _produtoSelecionado = new Produto();
        public Produto ProdutoSelecionado
        {
            get { return _produtoSelecionado; }
            set
            {
                _produtoSelecionado = value;
                OnPropertyChanged();
            }
        }

        private Produto _produtoSelecionadoNaLista;
        public Produto ProdutoSelecionadoNaLista
        {
            get { return _produtoSelecionadoNaLista; }
            set
            {
                if (_produtoSelecionadoNaLista == value)
                    return;

                _produtoSelecionadoNaLista = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // Copia os dados do produto selecionado para o formulário.
                    ProdutoSelecionado = new Produto
                    {
                        Id = value.Id,
                        Nome = value.Nome,
                        Descricao = value.Descricao,
                        Quantidade = value.Quantidade,
                        Preco = value.Preco
                    };
                }
            }
        }

        public Command SalvarProdutoCommand { get; set; }
        public Command ExcluirProdutoCommand { get; set; }
        public Command VerHistoricoGeralCommand { get; set; }
        public Command SairCommand { get; set; }
        public Command NovoProdutoCommand { get; set; }

        public ProdutoViewModel()
        {
            _dbService = new DatabaseService();

            SalvarProdutoCommand = new Command<Produto>(SalvarProduto);
            ExcluirProdutoCommand = new Command<Produto>(ExcluirProduto);
            VerHistoricoGeralCommand = new Command(CarregarHistoricoGeral);
            SairCommand = new Command(FazerLogout);
            NovoProdutoCommand = new Command(NovoProduto);

            CarregarProdutos();
            CarregarHistoricoGeral();
        }

        public void CarregarProdutos()
        {
            ListaProdutos = _dbService.ObterProdutos();
        }

        private void SalvarProduto(Produto produto)
        {
            if (produto == null)
            {
                Application.Current.MainPage.DisplayAlert(
                    "Aviso",
                    "Nenhum produto foi informado.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(produto.Nome))
            {
                Application.Current.MainPage.DisplayAlert(
                    "Aviso",
                    "O nome do produto é obrigatório.",
                    "OK");
                return;
            }

            bool novoProduto = produto.Id == 0;

            _dbService.SalvarProduto(produto);

            CarregarProdutos();

            // Limpa o formulário e a seleção após salvar.
            NovoProduto();

            string mensagem = novoProduto
                ? "Novo produto cadastrado com sucesso!"
                : "Produto atualizado com sucesso!";

            Application.Current.MainPage.DisplayAlert(
                "Sucesso",
                mensagem,
                "OK");
        }
        private void NovoProduto()
        {
            // Remove a seleção da lista.
            ProdutoSelecionadoNaLista = null;

            // Cria um produto com Id igual a zero.
            // Isso fará com que o banco realize um INSERT.
            ProdutoSelecionado = new Produto();
        }
        private async void ExcluirProduto(Produto produto)
        {
            if (produto != null && produto.Id != 0)
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar exclusão",
                    $"Deseja excluir o produto {produto.Nome}?",
                    "Sim",
                    "Não");

                if (!confirmar)
                    return;

                _dbService.ExcluirProduto(produto);
                CarregarProdutos();
                ProdutoSelecionado = new Produto();

                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso",
                    "Produto excluído com sucesso!",
                    "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Aviso",
                    "Selecione um produto para excluir.",
                    "OK");
            }
        }

        private void CarregarHistoricoGeral()
        {
            ListaHistorico = _dbService.ObterHistoricoTodos();
        }

        private void FazerLogout()
        {
            UsuarioAtual.Instancia.Logout();
            Application.Current.MainPage = new NavigationPage(new Views.LoginPage());
        }
    }
}