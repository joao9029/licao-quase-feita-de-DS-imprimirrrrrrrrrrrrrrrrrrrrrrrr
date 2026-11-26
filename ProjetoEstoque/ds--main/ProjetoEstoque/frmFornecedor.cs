using ProjetoEstoque.bancoDataSet1TableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetoEstoque
{
    public partial class frmFornecedor : Form
    {
        private Cadastro cad;

        public frmFornecedor()
        {
            InitializeComponent();
        }

        private void tb_fornecedorBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tb_fornecedorBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.bancoDataSet1);

        }

        private void frmFornecedor_Load(object sender, EventArgs e)
        {
            if (!Cadastro.Carregar(this, () => tb_fornecedorTableAdapter.Fill(bancoDataSet1.tb_fornecedor))) return;
            sg_estadoTextBox.CharacterCasing = CharacterCasing.Upper;
            cad = new Cadastro(this, tb_fornecedorBindingNavigator, tb_fornecedorBindingSource,
                bancoDataSet1.tb_fornecedor,
                () => tb_fornecedorTableAdapter.Update(bancoDataSet1.tb_fornecedor),
                () => { bancoDataSet1.tb_fornecedor.Clear(); tb_fornecedorTableAdapter.Fill(bancoDataSet1.tb_fornecedor); },
                ValidarFornecedor,
                new string[] { "nm_fornecedor", "cd_cnpj", "nm_cidade" },
                "Fornecedores", "nome, CNPJ ou cidade");
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            cad.Anterior();
        }

        private void btnProximo_Click(object sender, EventArgs e)
        {
            cad.Proximo();
        }

        private void btnNovo_Click(object sender, EventArgs e)
        {
            cad.Novo();
        }

        private void btnAlterar_Click(object sender, EventArgs e)
        {
            cad.Alterar();
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            cad.Excluir();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            cad.Salvar();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cad.Cancelar();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cad.Sair();
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            Relatorios.Fornecedores();
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            cad.Pesquisar();
        }

        private string ValidarFornecedor()
        {
            return Valida.Sequencia(
                () => Valida.Obrigatorio(nm_fornecedorTextBox, "o nome do fornecedor"),
                () => Valida.Uf(sg_estadoTextBox),
                () => Valida.Digitos(cd_cepTextBox, "O CEP", 8),
                () => Valida.Digitos(cd_cnpjTextBox, "O CNPJ", 14));
        }
    }
}
