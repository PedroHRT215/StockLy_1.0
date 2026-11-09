using Stockly.Models;

namespace Stockly.Singleton
{
    public class UsuarioAtual
    {
        private static UsuarioAtual _instancia;

        // Armazena os dados do usuário logado durante a execução do aplicativo
        public Usuario UsuarioLogado { get; private set; }

        // Construtor privado para impedir novas instâncias
        private UsuarioAtual() { }

        public static UsuarioAtual Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new UsuarioAtual();
                }
                return _instancia;
            }
        }

        public void SetUsuario(Usuario usuario)
        {
            UsuarioLogado = usuario;
        }

        public void Logout()
        {
            UsuarioLogado = null;
        }

    }
}