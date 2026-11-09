using SQLite;

namespace Stockly.Models
{
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string CPF { get; set; }

        public string Email { get; set; }

        // A propriedade armazenará o hash da senha,
        // e não mais a senha original.
        public string Senha { get; set; }

        // Administrador ou Cliente
        public string Tipo { get; set; }
    }

    public class Produto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Descricao { get; set; }

        public int Quantidade { get; set; }

        public decimal Preco { get; set; }
    }

    public class Historico
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int ProdutoId { get; set; }

        public string NomeUsuario { get; set; }

        public string NomeProduto { get; set; }

        public int Quantidade { get; set; }

        public DateTime DataHora { get; set; }
    }
}