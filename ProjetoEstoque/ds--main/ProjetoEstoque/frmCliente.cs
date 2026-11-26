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
    public partial class frmCliente : Form
    {
        private Cadastro cad;

        public frmCliente()
        {
            InitializeComponent();
        }

        private void tb_clienteBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tb_clienteBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.bancoDataSet1);

        }

        private void frmCliente_Load(object sender, EventArgs e)
        {
            if (!Cadastro.Carregar(this, () => tb_clienteTableAdapter.Fill(bancoDataSet1.tb_cliente))) return;
            sg_estadoTextBox.CharacterCasing = CharacterCasing.Upper;
            cad = new Cadastro(this, tb_clienteBindingNavigator, tb_clienteBindingSource,
                bancoDataSet1.tb_cliente,
                () => tb_clienteTableAdapter.Update(bancoDataSet1.tb_cliente),
                () => { bancoDataSet1.tb_cliente.Clear(); tb_clienteTableAdapter.Fill(bancoDataSet1.tb_cliente); },
                ValidarCliente,
                new string[] { "nm_cliente", "cd_cpf", "nm_cidade" },
                "Clientes", "nome, CPF ou cidade");
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            cad.Anterior();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            cad.Cancelar();
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            Relatorios.Clientes();
        }

        private void btnProximo_Click(object sender, EventArgs e)
        {
            cad.Proximo();
        }

        private void btnNovo_Click(object sender, EventArgs e)
        {
            cad.Novo();
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            cad.Pesquisar();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cad.Sair();
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

        private string ValidarCliente()
        {
            return Valida.Sequencia(
                () => Valida.Obrigatorio(nm_clienteTextBox, "o nome do cliente"),
                () => Valida.Uf(sg_estadoTextBox),
                () => Valida.Digitos(cd_cepTextBox, "O CEP", 8),
                () => Valida.Digitos(cd_cpfTextBox, "O CPF", 11));
        }
    }
}
