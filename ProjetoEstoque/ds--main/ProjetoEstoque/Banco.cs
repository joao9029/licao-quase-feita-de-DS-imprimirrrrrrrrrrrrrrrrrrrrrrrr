using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ProjetoEstoque
{
    internal static class Banco
    {
        private static bool pronto;

        public static void Preparar()
        {
            if (pronto) return;

            using (SqlConnection cn = new SqlConnection(Properties.Settings.Default.bancoConnectionString))
            {
                cn.Open();

                Garantir(cn, "tb_cliente", "cd_cliente", new string[] {
                    "nm_cliente|VARCHAR|50", "ds_endereco|VARCHAR|50", "nm_bairro|VARCHAR|50",
                    "nm_cidade|VARCHAR|30", "sg_estado|VARCHAR|2", "cd_cep|VARCHAR|8",
                    "cd_cpf|VARCHAR|14", "cd_rg|VARCHAR|15" }, new string[0]);

                Garantir(cn, "tb_fornecedor", "cd_fornecedor", new string[] {
                    "nm_fornecedor|VARCHAR|50", "ds_endereco|VARCHAR|50", "nm_bairro|VARCHAR|30",
                    "nm_cidade|VARCHAR|30", "sg_estado|VARCHAR|2", "cd_cep|VARCHAR|9",
                    "cd_cnpj|VARCHAR|18", "cd_ie|VARCHAR|15" }, new string[0]);

                Garantir(cn, "tb_usuario", "cd_usuario", new string[] {
                    "nm_usuario|VARCHAR|100", "sg_nivel|VARCHAR|1", "nm_login|VARCHAR|15",
                    "cd_senha|VARCHAR|15" }, new string[0]);

                Garantir(cn, "tb_produtos", "cd_produto", new string[] {
                    "nm_produto|VARCHAR|50", "sg_unidade_venda_produto|VARCHAR|2",
                    "vl_custo|DECIMAL(18,2)|0", "vl_venda|DECIMAL(18,2)|0", "qt_estoque|INT|0" },
                    new string[] { "sg_unidade_venda>sg_unidade_venda_produto" });
            }

            pronto = true;
        }

        public static string Explicar(Exception ex)
        {
            string m = ex.Message;
            bool conexao =
                m.IndexOf("LocalDB", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.IndexOf("Local Database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.IndexOf("rede", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0;

            if (conexao)
                return "Não foi possível conectar ao SQL Server LocalDB.\n\n" +
                       "Instale o componente SQL Server Express LocalDB pelo Visual Studio Installer " +
                       "(Modificar > Componentes individuais) e tente novamente.\n\n" +
                       "Detalhe: " + m;

            return "Não foi possível acessar o banco de dados.\n\nDetalhe: " + m;
        }

        private static void Garantir(SqlConnection cn, string tabela, string chave,
                                     string[] colunas, string[] renomear)
        {
            bool existe = Contar(cn,
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '" + tabela + "'") > 0;

            if (existe)
            {
                bool identidade = Contar(cn,
                    "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID('dbo." + tabela + "'), '" + chave + "', 'IsIdentity'), 0)") == 1;

                if (!identidade)
                {
                    if (Contar(cn, "SELECT COUNT(*) FROM dbo.[" + tabela + "]") > 0) return;
                    Executar(cn, "DROP TABLE dbo.[" + tabela + "]");
                    existe = false;
                }
            }

            if (!existe)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("CREATE TABLE dbo.[" + tabela + "] ([" + chave + "] INT IDENTITY(1,1) NOT NULL PRIMARY KEY");
                foreach (string def in colunas)
                {
                    string[] p = def.Split('|');
                    sb.Append(", [" + p[0] + "] " + Tipo(p[1], int.Parse(p[2])) + " NULL");
                }
                sb.Append(")");
                Executar(cn, sb.ToString());
                return;
            }

            foreach (string def in colunas)
            {
                string[] p = def.Split('|');
                string nome = p[0];
                string tipo = p[1];
                int tamanho = int.Parse(p[2]);

                string tipoAtual;
                int tamanhoAtual;

                if (!Coluna(cn, tabela, nome, out tipoAtual, out tamanhoAtual))
                {
                    string antigo = null;
                    foreach (string r in renomear)
                    {
                        string[] par = r.Split('>');
                        string t2;
                        int n2;
                        if (par[1] == nome && Coluna(cn, tabela, par[0], out t2, out n2)) antigo = par[0];
                    }

                    if (antigo != null)
                        Executar(cn, "EXEC sp_rename 'dbo." + tabela + "." + antigo + "', '" + nome + "', 'COLUMN'");
                    else
                        Executar(cn, "ALTER TABLE dbo.[" + tabela + "] ADD [" + nome + "] " + Tipo(tipo, tamanho) + " NULL");
                    continue;
                }

                if (tipo == "VARCHAR")
                {
                    if (!(tipoAtual == "varchar" && tamanhoAtual == tamanho))
                        Converter(cn, tabela, nome, "VARCHAR(" + tamanho + ")", tamanho);
                }
                else
                {
                    string baseEsperada = tipo.StartsWith("DECIMAL") ? "decimal" : "int";
                    if (tipoAtual != baseEsperada)
                        Converter(cn, tabela, nome, tipo, 0);
                }
            }

            TornarOpcionais(cn, tabela, chave);
        }

        private static void Converter(SqlConnection cn, string tabela, string coluna,
                                      string tipo, int tamanhoTexto)
        {
            try
            {
                if (tamanhoTexto > 0)
                    Executar(cn, "UPDATE dbo.[" + tabela + "] SET [" + coluna + "] = LEFT(RTRIM(CONVERT(VARCHAR(MAX), [" +
                                 coluna + "])), " + tamanhoTexto + ") WHERE [" + coluna + "] IS NOT NULL");

                Executar(cn, "ALTER TABLE dbo.[" + tabela + "] ALTER COLUMN [" + coluna + "] " + tipo + " NULL");
            }
            catch (SqlException)
            {
            }
        }

        private static void TornarOpcionais(SqlConnection cn, string tabela, string chave)
        {
            List<string> comandos = new List<string>();

            using (SqlCommand cmd = new SqlCommand(
                "SELECT COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH, 0), " +
                "ISNULL(NUMERIC_PRECISION, 0), ISNULL(NUMERIC_SCALE, 0) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @t AND IS_NULLABLE = 'NO' AND COLUMN_NAME <> @k", cn))
            {
                cmd.Parameters.AddWithValue("@t", tabela);
                cmd.Parameters.AddWithValue("@k", chave);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string nome = Convert.ToString(dr[0]);
                        string tipo = Convert.ToString(dr[1]).ToLowerInvariant();
                        int tam = Convert.ToInt32(dr[2]);
                        int prec = Convert.ToInt32(dr[3]);
                        int esc = Convert.ToInt32(dr[4]);

                        string completo = tipo;
                        if (tipo == "varchar" || tipo == "nvarchar" || tipo == "char" || tipo == "nchar" ||
                            tipo == "varbinary" || tipo == "binary")
                        {
                            if (tam == -1) completo = tipo + "(MAX)";
                            else if (tam > 0) completo = tipo + "(" + tam + ")";
                        }
                        else if (tipo == "decimal" || tipo == "numeric")
                        {
                            completo = tipo + "(" + prec + "," + esc + ")";
                        }

                        comandos.Add("ALTER TABLE dbo.[" + tabela + "] ALTER COLUMN [" + nome + "] " + completo + " NULL");
                    }
                }
            }

            foreach (string sql in comandos)
            {
                try
                {
                    Executar(cn, sql);
                }
                catch (SqlException)
                {
                }
            }
        }

        private static string Tipo(string tipo, int tamanho)
        {
            return tamanho > 0 ? tipo + "(" + tamanho + ")" : tipo;
        }

        private static bool Coluna(SqlConnection cn, string tabela, string coluna,
                                   out string tipo, out int tamanho)
        {
            tipo = null;
            tamanho = 0;
            using (SqlCommand cmd = new SqlCommand(
                "SELECT DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @t AND COLUMN_NAME = @c", cn))
            {
                cmd.Parameters.AddWithValue("@t", tabela);
                cmd.Parameters.AddWithValue("@c", coluna);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return false;
                    tipo = Convert.ToString(dr[0]).ToLowerInvariant();
                    tamanho = Convert.ToInt32(dr[1]);
                    return true;
                }
            }
        }

        private static int Contar(SqlConnection cn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void Executar(SqlConnection cn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
