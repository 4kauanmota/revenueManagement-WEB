using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using CadastroFoodApi.Models;
using Dapper;

namespace CadastroFoodApi.DAOs
{
    public abstract class AutoDAO<T> where T : BaseModel, new()
    {

        protected abstract string Tabela { get; }

        public virtual async Task InserirAsync(T obj)
        {
            //Falta fazer o retorno dos campos em acordo com o objeto
            var sql = $"INSERT INTO {Tabela} ({GetInsertCampos()}) VALUES ({GetInsertValores()})";
            
            if (string.IsNullOrEmpty(obj.Id))
                obj.Id = Guid.NewGuid().ToString();

            await ExecutarComandoAsync(sql, obj);
        }

        public virtual async Task AlterarAsync(T obj)
        {
            //Falta fazer o retorno dos campos em acordo com o objeto
            var sql = $"UPDATE {Tabela} SET {GetUpdateCampos()} WHERE Id=@Id";

            await ExecutarComandoAsync(sql, obj);
        }

        public virtual async Task ExcluirAsync(string id)
        {
            var sql = $"DELETE FROM {Tabela} WHERE Id=@Id";

            await ExecutarComandoAsync(sql, new { Id = id });
        }
        
        public virtual async Task ExcluirFoodIngredientAsync(string idFood, string idIngredient)
        {
            var sql = $"DELETE FROM {Tabela} WHERE IdFood=@IdFood and IdIngredient=@IdIngredient";

            await ExecutarComandoAsync(sql, new { IdFood = idFood, IdIngredient = idIngredient });
        }

        public virtual async Task<IList<T>> RetornarTodosAsync()
        {
            return await ExecutarConsultaAsync($"SELECT * FROM {Tabela}");
        }
        
        public virtual async Task<IList<T>> RetornarTodosAsyncWhereID(string id)
        {
            return await ExecutarConsultaAsync($"SELECT * FROM {Tabela} WHERE IdFood=Id", new { Id=id });
        }

        public virtual async Task<T?> RetornarPorIdAsync(string id)
        {
            return await ExecutarConsultaObjetoUnicoAsync($"SELECT * FROM {Tabela} WHERE Id=@Id", new { Id=id });
        }

        private async Task<IList<T>> ExecutarConsultaAsync(string sql, object? obj = null)
        {
            IList<T>? objetos = null;

            var stringConexao = RetornarConnectionString();

            using (var conexao = new MySqlConnection(stringConexao))
            {
                conexao.Open();

                objetos = (await conexao.QueryAsync<T>(sql)).ToList();

                conexao.Close();
            }

            return objetos ?? new List<T>();
        }

        private async Task<T?> ExecutarConsultaObjetoUnicoAsync(string sql, object? obj = null)
        {
            T? objeto = null;

            var stringConexao = RetornarConnectionString();

            using (var conexao = new MySqlConnection(stringConexao))
            {
                conexao.Open();

                objeto = await conexao.QueryFirstAsync<T>(sql, obj);

                conexao.Close();
            }

            return objeto;
        }

        private async Task ExecutarComandoAsync(string sql, object obj)
        {
            var stringConexao = RetornarConnectionString();

            using (var conexao = new MySqlConnection(stringConexao))
            {
                conexao.Open();

                await conexao.ExecuteAsync(sql, obj);

                conexao.Close();
            }
        }

        private string GetInsertCampos()
        {
            var sb = new StringBuilder();
            var propriedades = GetPropriedades();

            sb.Append(propriedades[0]);

            for (int i = 1; i < propriedades.Length; i++)
                sb.Append($",{propriedades[i]}");

            return sb.ToString();
        }

        private string GetInsertValores()
        {
            var sb = new StringBuilder();
            var propriedades = GetPropriedades();

            sb.Append($"@{propriedades[0]}");

            for (int i = 1; i < propriedades.Length; i++)
                sb.Append($",@{propriedades[i]}");

            return sb.ToString();
        }

        private string GetUpdateCampos()
        {
            var sb = new StringBuilder();
            var propriedades = GetPropriedades().Where(x => !x.Equals("Id")).ToArray();

            sb.Append($"{propriedades[0]}=@{propriedades[0]}");

            for (int i = 1; i < propriedades.Length; i++)
                sb.Append($",{propriedades[i]}=@{propriedades[i]}");

            return sb.ToString();
        }

        private string[] GetPropriedades()
        {
            if (nomesPropriedades == null)
            {            
                var tipo = typeof(T);

                var lista = new List<string>();

                foreach (var prop in tipo.GetProperties())
                    lista.Add(prop.Name);

                nomesPropriedades = lista.ToArray();
            }

            return nomesPropriedades;
        }

        private static string[] nomesPropriedades = null;

        private string RetornarConnectionString()
        {
            //return "SERVER=localhost;DATABASE=teste;UID=root;PASSWORD=aluno;"
            string server = "localhost";
            string database = "icomida";
            string uid = "aluno";
            string password = "aluno";
            
            string connectionString = 
            "SERVER=" + server + ";" +
            "DATABASE=" + database + ";" + 
            "UID=" + uid + ";" +
            "PASSWORD=" + password + ";";

            return connectionString;
        }
    }
}