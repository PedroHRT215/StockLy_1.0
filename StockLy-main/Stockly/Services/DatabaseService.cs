using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Stockly.Models;

namespace Stockly.Services
{
    public class DatabaseService
    {
        private static SQLiteConnection _db;

        private static readonly object _bloqueio =
            new object();

        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public DatabaseService()
        {
            lock (_bloqueio)
            {
                if (_db == null)
                {
                    string dbPath = Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData),
                        "Stockly.db3");

                    _db = new SQLiteConnection(dbPath);

                    // Cria as tabelas caso ainda não existam
                    _db.CreateTable<Usuario>();

                    _db.CreateTable<Produto>();

                    _db.CreateTable<Historico>();

                    // Atualiza a estrutura do histórico antigo
                    AtualizarTabelaHistorico();
                }
            }
        }

        // =========================================================
        // MIGRAÇÃO DO HISTÓRICO
        // =========================================================

        private void AtualizarTabelaHistorico()
        {
            var colunas = _db.GetTableInfo("Historico");

            bool possuiQuantidade = colunas.Any(
                coluna => coluna.Name == "Quantidade");

            if (!possuiQuantidade)
            {
                _db.Execute(
                    "ALTER TABLE Historico " +
                    "ADD COLUMN Quantidade INTEGER NOT NULL DEFAULT 1");
            }
        }

        // =========================================================
        // NORMALIZAR EMAIL
        // =========================================================

        private string NormalizarEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        // =========================================================
        // BUSCAR USUÁRIO PELO EMAIL
        // =========================================================

        private Usuario BuscarUsuarioPorEmail(string email)
        {
            return _db.Query<Usuario>(
                "SELECT * FROM Usuario " +
                "WHERE Email = ? COLLATE NOCASE LIMIT 1",
                email.Trim()).FirstOrDefault();
        }

        // =========================================================
        // VERIFICAR EMAIL EXISTENTE
        // =========================================================

        public bool EmailJaCadastrado(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            lock (_bloqueio)
            {
                return BuscarUsuarioPorEmail(
                    email) != null;
            }
        }

        // =========================================================
        // CADASTRAR USUÁRIO
        // =========================================================

        public bool SalvarUsuario(Usuario usuario)
        {
            if (usuario == null ||
                string.IsNullOrWhiteSpace(usuario.Email) ||
                string.IsNullOrWhiteSpace(usuario.Senha))
            {
                return false;
            }

            lock (_bloqueio)
            {
                usuario.Email = NormalizarEmail(usuario.Email);

                // Impede e-mail duplicado
                if (BuscarUsuarioPorEmail(usuario.Email) != null)
                {
                    return false;
                }

                // Todo cadastro público será Cliente
                usuario.Tipo = "Cliente";

                // Protege a senha antes de salvar
                usuario.Senha = SenhaService.GerarHash(
                    usuario.Senha);

                return _db.Insert(usuario) > 0;
            }
        }

        // =========================================================
        // CRIAR ADMINISTRADOR INICIAL
        // =========================================================

        public bool CriarAdministradorInicial(Usuario usuario)
        {
            if (usuario == null ||
                string.IsNullOrWhiteSpace(usuario.Email) ||
                string.IsNullOrWhiteSpace(usuario.Senha))
    {
                return false;
            }

            string email = usuario.Email.Trim().ToLowerInvariant();

            Usuario existente = _db.Table<Usuario>()
                .ToList()
                .FirstOrDefault(u =>
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    u.Email.Trim().ToLowerInvariant() == email);

            string senhaHash = SenhaService.GerarHash(usuario.Senha);

            if (existente != null)
            {
                existente.Nome = usuario.Nome;
                existente.CPF = usuario.CPF;
                existente.Email = email;
                existente.Senha = senhaHash;
                existente.Tipo = "Administrador";

                _db.Update(existente);
                return true;
            }

            usuario.Email = email;
            usuario.Senha = senhaHash;
            usuario.Tipo = "Administrador";

            _db.Insert(usuario);

            return true;
        }

        // =========================================================
        // VALIDAR LOGIN
        // =========================================================

        public Usuario ValidarLogin(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                return null;
            }

            lock (_bloqueio)
            {
                email = NormalizarEmail(email);

                Usuario usuario = BuscarUsuarioPorEmail(email);

                if (usuario == null)
                {
                    return null;
                }

                // Verifica a senha usando o SenhaService
                bool senhaValida = SenhaService.VerificarSenha(
                    senha,
                    usuario.Senha);

                if (!senhaValida)
                {
                    return null;
                }

                // Se a senha for antiga, converte para hash
                if (!SenhaService.EstaProtegida(usuario.Senha))
                {
                    usuario.Senha = SenhaService.GerarHash(senha);

                    _db.Update(usuario);
                }

                return usuario;
            }
        }

        // =========================================================
        // BUSCAR USUÁRIO PELO ID
        // =========================================================

        public Usuario GetUsuarioPorId(int id)
        {
            lock (_bloqueio)
            {
                return _db.Find<Usuario>(id);
            }
        }

        // =========================================================
        // MÉTODOS PARA PRODUTOS
        // =========================================================

        public List<Produto> ObterProdutos()
        {
            lock (_bloqueio)
            {
                return _db.Table<Produto>().ToList();
            }
        }

        public void SalvarProduto(Produto produto)
        {
            if (produto.Id != 0)
                _db.Update(produto);
            else
                _db.Insert(produto);
        }

        public void ExcluirProduto(Produto produto)
        {
            lock (_bloqueio)
            {
                _db.Delete(produto);
            }
        }

        // =========================================================
        // REGISTRAR RESERVA E ATUALIZAR ESTOQUE
        // =========================================================

        public bool RegistrarReserva(
            Usuario usuario,
            int produtoId,
            int quantidade)
        {
            if (usuario == null ||
                produtoId <= 0 ||
                quantidade <= 0)
            {
                return false;
            }

            lock (_bloqueio)
            {
                bool reservaRealizada = false;

                _db.RunInTransaction(() =>
                {
                    // Busca novamente o produto no banco.
                    // Isso evita utilizar um estoque desatualizado.
                    Produto produto = _db.Find<Produto>(produtoId);

                    if (produto == null ||
                        produto.Quantidade < quantidade)
                    {
                        return;
                    }

                    // Baixa o estoque
                    produto.Quantidade -= quantidade;

                    _db.Update(produto);

                    // Registra a movimentação
                    Historico historico = new Historico
                    {
                        UsuarioId = usuario.Id,

                        ProdutoId = produto.Id,

                        NomeUsuario = usuario.Nome,

                        NomeProduto = produto.Nome,

                        Quantidade = quantidade,

                        DataHora = DateTime.Now
                    };

                    _db.Insert(historico);

                    reservaRealizada = true;
                });

                return reservaRealizada;
            }
        }

        // =========================================================
        // MÉTODOS PARA HISTÓRICO
        // =========================================================

        public void RegistrarHistorico(Historico historico)
        {
            lock (_bloqueio)
            {
                _db.Insert(historico);
            }
        }

        public List<Historico> ObterHistoricoTodos()
        {
            lock (_bloqueio)
            {
                return _db.Table<Historico>()
                    .OrderByDescending(h => h.DataHora)
                    .ToList();
            }
        }

        public List<Historico> ObterHistoricoPorUsuario(int usuarioId)
        {
            lock (_bloqueio)
            {
                return _db.Table<Historico>()
                    .Where(h => h.UsuarioId == usuarioId)
                    .OrderByDescending(h => h.DataHora)
                    .ToList();
            }
        }

        public List<Historico> GetHistoricoPorUsuarioId(int usuarioId)
        {
            return ObterHistoricoPorUsuario(usuarioId);
        }
    }
}