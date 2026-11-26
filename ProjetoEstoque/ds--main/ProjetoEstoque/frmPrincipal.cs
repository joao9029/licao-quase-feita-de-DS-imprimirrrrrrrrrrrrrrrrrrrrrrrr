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
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
            Tema.Aplicar(this);
        }
        private void relatórioToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void usuáriosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmUsuario fu = new frmUsuario();
            fu.ShowDialog();
        } 

        private void cadastroToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clientesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmFornecedor ff = new frmFornecedor();
            ff.ShowDialog();
        }

        private void clientesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmCliente fc = new frmCliente();
            fc.ShowDialog();
        }

        private void produtosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmProduto fp = new frmProduto();
            fp.ShowDialog();
        }

        private void relatorioUsuarios_Click(object sender, EventArgs e)
        {
            Relatorios.Usuarios();
        }

        private void relatorioFornecedores_Click(object sender, EventArgs e)
        {
            Relatorios.Fornecedores();
        }

        private void relatorioClientes_Click(object sender, EventArgs e)
        {
            Relatorios.Clientes();
        }

        private void relatorioProdutos_Click(object sender, EventArgs e)
        {
            Relatorios.Produtos();
        }

    }
}
