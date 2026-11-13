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
    public partial class frmUsuario : Form
    {
        private Cadastro cad;

        public frmUsuario()
        {
            InitializeComponent();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            cad.Sair();
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void frmUsuario_Load(object sender, EventArgs e)
        {
            if (!Cadastro.Carregar(this, () => tb_usuarioTableAdapter.Fill(bancoDataSet1.tb_usuario))) return;
            sg_nivelTextBox.CharacterCasing = CharacterCasing.Upper;
            cd_senhaTextBox.UseSystemPasswordChar = true;
            cad = new Cadastro(this, tb_usuarioBindingNavigator, tb_usuarioBindingSource,
                bancoDataSet1.tb_usuario,
                () => tb_usuarioTableAdapter.Update(bancoDataSet1.tb_usuario),
                ValidarUsuario,
                new string[] { "nm_usuario", "nm_login" },
                "Usuários", "nome ou login");
        }

        private void bindingSource2_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxCodigo_TextChanged(object sender, EventArgs e)
        {

        }
            
        private void txtBoxNome_TextChanged(object sender, EventArgs e)
        {

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

        private void txtBoxSenha_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxNivel_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtBoxLogin_TextChanged(object sender, EventArgs e)
        {

        }

        private void tb_usuarioBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tb_usuarioBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.bancoDataSet1);

        }

        private void cd_usuarioLabel_Click(object sender, EventArgs e)
        {

        }

        private void nm_usuarioTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            Impressao.Imprimir("Cadastro de Usuários",
                tb_usuarioTableAdapter.GetData(),
                new string[] { "cd_usuario", "nm_usuario", "sg_nivel", "nm_login" },
                new string[] { "Código", "Nome", "Nível", "Login" },
                new float[] { 8, 40, 12, 25 });
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            cad.Pesquisar();
        }

        private string ValidarUsuario()
        {
            return Valida.Sequencia(
                () => Valida.Obrigatorio(nm_usuarioTextBox, "o nome do usuário"),
                () => Valida.Obrigatorio(sg_nivelTextBox, "o nível (1 caractere)"),
                () => Valida.Obrigatorio(nm_loginTextBox, "o login"),
                () => Valida.Obrigatorio(cd_senhaTextBox, "a senha"),
                LoginRepetido);
        }

        private string LoginRepetido()
        {
            string login = nm_loginTextBox.Text.Trim();
            DataRowView atual = tb_usuarioBindingSource.Current as DataRowView;
            foreach (DataRow r in bancoDataSet1.tb_usuario.Rows)
            {
                if (r.RowState == DataRowState.Deleted || r.RowState == DataRowState.Detached) continue;
                if (atual != null && ReferenceEquals(r, atual.Row)) continue;
                if (string.Equals(Convert.ToString(r["nm_login"]).Trim(), login, StringComparison.OrdinalIgnoreCase))
                {
                    nm_loginTextBox.Focus();
                    return "Já existe um usuário com o login \"" + login + "\". Escolha outro.";
                }
            }
            return null;
        }
    }
}
