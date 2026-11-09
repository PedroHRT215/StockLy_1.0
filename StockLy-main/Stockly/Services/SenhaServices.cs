using System;
using System.Security.Cryptography;

namespace Stockly.Services
{
    public static class SenhaService
    {
        private const int Iteracoes = 600000;

        private const int TamanhoSalt = 16;

        private const int TamanhoHash = 32;

        private const string Algoritmo = "PBKDF2-SHA256";

        // =========================================================
        // GERAR HASH DA SENHA
        // =========================================================

        public static string GerarHash(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
            {
                throw new ArgumentException(
                    "A senha não pode estar vazia.");
            }

            // Gera um salt aleatório para cada usuário
            byte[] salt = RandomNumberGenerator.GetBytes(
                TamanhoSalt);

            // Gera o hash utilizando PBKDF2
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                senha,
                salt,
                Iteracoes,
                HashAlgorithmName.SHA256,
                TamanhoHash);

            // Organiza os dados para armazenamento
            return $"{Algoritmo}$" +
                   $"{Iteracoes}$" +
                   $"{Convert.ToBase64String(salt)}$" +
                   $"{Convert.ToBase64String(hash)}";
        }

        // =========================================================
        // VERIFICAR SENHA
        // =========================================================

        public static bool VerificarSenha(
            string senhaDigitada,
            string senhaArmazenada)
        {
            if (string.IsNullOrEmpty(senhaDigitada) ||
                string.IsNullOrEmpty(senhaArmazenada))
            {
                return false;
            }

            // Compatibilidade com senhas antigas
            // que ainda estão armazenadas em texto puro.
            if (!EstaProtegida(senhaArmazenada))
            {
                return string.Equals(
                    senhaDigitada,
                    senhaArmazenada,
                    StringComparison.Ordinal);
            }

            try
            {
                string[] partes = senhaArmazenada.Split('$');

                if (partes.Length != 4)
                {
                    return false;
                }

                if (partes[0] != Algoritmo)
                {
                    return false;
                }

                if (!int.TryParse(partes[1], out int iteracoes))
                {
                    return false;
                }

                if (iteracoes <= 0 || iteracoes > 1000000)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(partes[2]);

                byte[] hashArmazenado =
                    Convert.FromBase64String(partes[3]);

                // Recalcula o hash usando a senha digitada
                byte[] hashDigitado = Rfc2898DeriveBytes.Pbkdf2(
                    senhaDigitada,
                    salt,
                    iteracoes,
                    HashAlgorithmName.SHA256,
                    hashArmazenado.Length);

                // Compara sem revelar diferenças por tempo de execução
                return CryptographicOperations.FixedTimeEquals(
                    hashDigitado,
                    hashArmazenado);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        // =========================================================
        // IDENTIFICAR SE A SENHA JÁ ESTÁ PROTEGIDA
        // =========================================================

        public static bool EstaProtegida(string senha)
        {
            return !string.IsNullOrEmpty(senha) &&
                   senha.StartsWith(
                       Algoritmo + "$",
                       StringComparison.Ordinal);
        }
    }
}