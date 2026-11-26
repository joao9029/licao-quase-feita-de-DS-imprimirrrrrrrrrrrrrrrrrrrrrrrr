using System;
using System.Data;
using System.Windows.Forms;
using ProjetoEstoque.bancoDataSet1TableAdapters;

namespace ProjetoEstoque
{
    internal static class Relatorios
    {
        public static void Clientes()
        {
            Gerar("Relatório de Clientes",
                () => { using (tb_clienteTableAdapter ta = new tb_clienteTableAdapter()) return (DataTable)ta.GetData(); },
                new string[] { "cd_cliente", "nm_cliente", "ds_endereco", "nm_bairro", "nm_cidade", "sg_estado", "cd_cep", "cd_cpf", "cd_rg" },
                new string[] { "Código", "Nome", "Endereço", "Bairro", "Cidade", "UF", "CEP", "CPF", "RG" },
                new float[] { 6, 20, 20, 12, 12, 4, 9, 11, 9 });
        }

        public static void Fornecedores()
        {
            Gerar("Relatório de Fornecedores",
                () => { using (tb_fornecedorTableAdapter ta = new tb_fornecedorTableAdapter()) return (DataTable)ta.GetData(); },
                new string[] { "cd_fornecedor", "nm_fornecedor", "ds_endereco", "nm_bairro", "nm_cidade", "sg_estado", "cd_cep", "cd_cnpj", "cd_ie" },
                new string[] { "Código", "Nome", "Endereço", "Bairro", "Cidade", "UF", "CEP", "CNPJ", "IE" },
                new float[] { 6, 20, 20, 12, 12, 4, 9, 12, 9 });
        }

        public static void Produtos()
        {
            Gerar("Relatório de Produtos",
                () => { using (tb_produtosTableAdapter ta = new tb_produtosTableAdapter()) return (DataTable)ta.GetData(); },
                new string[] { "cd_produto", "nm_produto", "sg_unidade_venda_produto", "vl_custo", "vl_venda", "qt_estoque" },
                new string[] { "Código", "Produto", "Unidade", "Custo", "Venda", "Estoque" },
                new float[] { 8, 36, 10, 12, 12, 10 });
        }

        public static void Usuarios()
        {
            Gerar("Relatório de Usuários",
                () => { using (tb_usuarioTableAdapter ta = new tb_usuarioTableAdapter()) return (DataTable)ta.GetData(); },
                new string[] { "cd_usuario", "nm_usuario", "sg_nivel", "nm_login" },
                new string[] { "Código", "Nome", "Nível", "Login" },
                new float[] { 8, 40, 12, 25 });
        }

        private static void Gerar(string titulo, Func<DataTable> obter, string[] campos,
                                  string[] cabecalhos, float[] larguras)
        {
            DataTable tabela;
            try
            {
                Banco.Preparar();
                tabela = obter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Banco.Explicar(ex), "Banco de dados",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Impressao.Imprimir(titulo, tabela, campos, cabecalhos, larguras);
        }
    }
}
