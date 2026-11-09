using Stockly.Models;
using Stockly.Services;
using Stockly.Singleton;
using Stockly.Views;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Stockly.ViewModels
{
    public class UsuarioViewModel : BaseNotifyViewModel
    {
        private DatabaseService _dbService;

        private string _email;
        public string Email
        {
            get { return _email; }
            set { _email = value; OnPropertyChanged(); }
        }

        private string _senha;
        public string Senha
        {
            get { return _senha; }
            set { _senha = value; OnPropertyChanged(); }
        }

        private string _confirmarSenha;
        public string ConfirmarSenha
        {
            get { return _confirmarSenha; }
            set { _confirmarSenha = value; OnPropertyChanged(); }
        }

        private string _nome;
        public string Nome
        {
            get { return _nome; }
            set { _nome = value; OnPropertyChanged(); }
        }

        private string _cpf;
        public string CPF
        {
            get { return _cpf; }
            set { _cpf = value; OnPropertyChanged(); }
        }


        // Propriedades para exibir a mensagem de erro na tela
        private string _mensagemValidacaoSenha;
        public string MensagemValidacaoSenha
        {
            get { return _mensagemValidacaoSenha; }
            set { _mensagemValidacaoSenha = value; OnPropertyChanged(); }
        }

        private bool _exibirMensagemErroSenha;
        public bool ExibirMensagemErroSenha
        {
            get { return _exibirMensagemErroSenha; }
            set { _exibirMensagemErroSenha = value; OnPropertyChanged(); }
        }

        public Command EntrarCommand
        {
            get { return new Command(() => { RealizarLogin(); }); }
        }

        public Command RegistrarCommand
        {
            get { return new Command(() => { Registrar(); }); }
        }

        public Command SalvarCadastroCommand
        {
            get { return new Command(() => { SalvarNovoUsuario(); }); }
        }

        public Command VoltarCommand
        {
            get { return new Command(() => { Voltar(); }); }
        }

        // =========================================================
        // PASSO 3: COMANDO PARA ABRIR O RESUMO DO USUÁRIO
        // =========================================================
        public Command<object> AbrirResumoUsuarioCommand
        {
            get
            {
                return new Command<object>(async (itemSelecionado) =>
                {
                    if (itemSelecionado != null)
                    {
                        var historico = itemSelecionado as Historico;

                        if (historico != null)
                        {
                            // Busca o usuário no banco usando o ID da reserva/movimentação
                            Usuario usuarioDaReserva = _dbService.GetUsuarioPorId(historico.UsuarioId);

                            if (usuarioDaReserva != null)
                            {
                                await Application.Current.MainPage.Navigation.PushAsync(new Views.ResumoReservaPage(usuarioDaReserva));
                            }
                        }
                    }
                });
            }
        }

        public UsuarioViewModel()
        {
            _dbService = new DatabaseService();
        }

        private void Registrar()
        {
            LimparCampos(); // Limpa antes de abrir a tela de registro
            Application.Current.MainPage = new NavigationPage(new RegistroPage());
        }

        private void Voltar()
        {
            LimparCampos(); // Limpa ao voltar para o Login
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        private void RealizarLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Application.Current.MainPage.DisplayAlert("Erro", "Preencha e-mail e senha.", "OK");
                return;
            }

            Usuario usuarioEncontrado = _dbService.ValidarLogin(Email, Senha);

            if (usuarioEncontrado != null)
            {
                UsuarioAtual.Instancia.SetUsuario(usuarioEncontrado);

                if (usuarioEncontrado.Tipo == "Administrador")
                {
                    Application.Current.MainPage.DisplayAlert("Sucesso", "Bem-vindo, Admin!", "OK");
                    Application.Current.MainPage = new NavigationPage(new AdminPage());
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Sucesso", "Bem-vindo, Cliente!", "OK");
                    Application.Current.MainPage = new NavigationPage(new ClientePage());
                }
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Erro", "E-mail ou senha incorretos.", "OK");
            }
        }

        private async void SalvarNovoUsuario()
        {
            if (string.IsNullOrWhiteSpace(Nome) ||
                string.IsNullOrWhiteSpace(CPF) ||
                string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Aviso",
                    "Preencha todos os campos obrigatórios.",
                    "OK");

                return;
            }

            if (!ValidarSenha())
            {
                return;
            }

            // Verifica se o e-mail já está cadastrado
            if (_dbService.EmailJaCadastrado(Email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Cadastro",
                    "Este e-mail já está cadastrado.",
                    "OK");

                return;
            }

            Usuario novoUsuario = new Usuario
            {
                Nome = Nome.Trim(),

                CPF = CPF.Trim(),

                Email = Email.Trim().ToLowerInvariant(),

                Senha = Senha,

                Tipo = "Cliente"
            };

            // O DatabaseService também verifica duplicidade
            // e protege a senha antes de inserir no SQLite.
            bool cadastroRealizado =
                _dbService.SalvarUsuario(novoUsuario);

            if (!cadastroRealizado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    "Não foi possível cadastrar o usuário.",
                    "OK");

                return;
            }

            await Application.Current.MainPage.DisplayAlert(
                "Sucesso",
                "Usuário cadastrado com sucesso!",
                "OK");

            Voltar();
        }


        private bool ValidarSenha()
        {
            if (string.IsNullOrWhiteSpace(Senha))
            {
                MensagemValidacaoSenha = "Campo obrigatório";
                ExibirMensagemErroSenha = true;
                return false;
            }
            else if (Senha.Length < 8)
            {
                MensagemValidacaoSenha = "Senha deve conter no mínimo 8 caracteres";
                ExibirMensagemErroSenha = true;
                return false;
            }

            else if (Senha != ConfirmarSenha)
            {
                MensagemValidacaoSenha = "As senhas não coincidem";
                ExibirMensagemErroSenha = true;
                return false;
            }
            else
            {
                ExibirMensagemErroSenha = false;
                MensagemValidacaoSenha = string.Empty;
                return true;
            }
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            CPF = string.Empty;
            Email = string.Empty;
            Senha = string.Empty;
            ConfirmarSenha = string.Empty;

            // Reseta a mensagem de erro ao sair
            ExibirMensagemErroSenha = false;
            MensagemValidacaoSenha = string.Empty;
        }
    }
}