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
    public partial class frmProduto : Form
    {
        private Cadastro cad;

        public frmProduto()
        {
            InitializeComponent();
        }

        private void tb_produtosBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tb_produtosBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.bancoDataSet1);
        }

        private void frmProduto_Load(object sender, EventArgs e)
        {
            if (!Cadastro.Carregar(this, () => tb_produtosTableAdapter.Fill(bancoDataSet1.tb_produtos))) return;
            sg_unidade_venda_produtoTextBox.CharacterCasing = CharacterCasing.Upper;
            cad = new Cadastro(this, tb_produtosBindingNavigator, tb_produtosBindingSource,
                bancoDataSet1.tb_produtos,
                () => tb_produtosTableAdapter.Update(bancoDataSet1.tb_produtos),
                () => { bancoDataSet1.tb_produtos.Clear(); tb_produtosTableAdapter.Fill(bancoDataSet1.tb_produtos); },
                ValidarProduto,
                new string[] { "nm_produto", "sg_unidade_venda_produto" },
                "Produtos", "nome do produto");
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

        private void vl_custoLabel_Click(object sender, EventArgs e)
        {

        }

        private void vl_custoTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void sg_unidade_venda_produtoTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void sg_unidade_venda_produtoLabel_Click(object sender, EventArgs e)
        {

        }

        private void qt_estoqueLabel_Click(object sender, EventArgs e)
        {

        }

        private void nm_produtoLabel_Click(object sender, EventArgs e)
        {

        }

        private void cd_produtoTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            Relatorios.Produtos();
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            cad.Pesquisar();
        }

        private string ValidarProduto()
        {
            return Valida.Sequencia(
                () => Valida.Obrigatorio(nm_produtoTextBox, "o nome do produto"),
                () => Valida.Obrigatorio(sg_unidade_venda_produtoTextBox, "a unidade de venda (ex.: UN, KG, CX)"),
                () => Valida.Decimal(vl_custoTextBox, "o valor de custo"),
                () => Valida.Decimal(vl_vendaTextBox, "o valor de venda"),
                () => Valida.Inteiro(qt_estoqueTextBox, "a quantidade em estoque"));
        }
    }
}