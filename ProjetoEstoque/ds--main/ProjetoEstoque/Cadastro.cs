using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ProjetoEstoque
{
    internal class Cadastro
    {
        private readonly Form form;
        private readonly BindingNavigator navegador;
        private readonly BindingSource fonte;
        private readonly DataTable tabela;
        private readonly Func<int> gravar;
        private readonly Action recarregar;
        private readonly Func<string> validar;
        private readonly string[] camposBusca;
        private readonly string nomeCadastro;
        private readonly string dicaBusca;

        private readonly List<TextBox> caixas = new List<TextBox>();
        private TextBox caixaChave;
        private DataColumn colunaChave;

        private readonly Button btnAnterior, btnProximo, btnNovo, btnAlterar, btnExcluir;
        private readonly Button btnSalvar, btnCancelar, btnPesquisar, btnImprimir;

        private bool editando;
        private string ultimaBusca = "";

        public Cadastro(Form form, BindingNavigator navegador, BindingSource fonte,
                        DataTable tabela, Func<int> gravar, Action recarregar, Func<string> validar,
                        string[] camposBusca, string nomeCadastro, string dicaBusca)
        {
            this.form = form;
            this.navegador = navegador;
            this.fonte = fonte;
            this.tabela = tabela;
            this.gravar = gravar;
            this.recarregar = recarregar;
            this.validar = validar;
            this.camposBusca = camposBusca;
            this.nomeCadastro = nomeCadastro;
            this.dicaBusca = dicaBusca;

            Tema.Aplicar(form);

            btnAnterior = Botao("btnAnterior");
            btnProximo = Botao("btnProximo");
            btnNovo = Botao("btnNovo");
            btnAlterar = Botao("btnAlterar");
            btnExcluir = Botao("btnExcluir");
            btnSalvar = Botao("btnSalvar");
            btnCancelar = Botao("btnCancelar");
            btnPesquisar = Botao("btnPesquisar");
            btnImprimir = Botao("btnImprimir");

            foreach (ToolStripItem item in navegador.Items)
            {
                string n = item.Name;
                if (n.Contains("AddNewItem") || n.Contains("DeleteItem") || n.Contains("SaveItem"))
                    item.Visible = false;
            }

            ColetarCaixas(form);
            caixas.Sort((a, b) => a.TabIndex.CompareTo(b.TabIndex));

            foreach (TextBox tb in caixas)
            {
                Binding b = tb.DataBindings["Text"];
                if (b == null) continue;

                DataColumn col = tabela.Columns[b.BindingMemberInfo.BindingField];
                if (col == null) continue;

                if (col.MaxLength > 0) tb.MaxLength = col.MaxLength;

                if (col.AutoIncrement)
                {
                    caixaChave = tb;
                    colunaChave = col;
                    b.DataSourceUpdateMode = DataSourceUpdateMode.Never;
                    b.Format += (s, ev) =>
                    {
                        if (ev.Value is int && (int)ev.Value < 0) ev.Value = "Novo";
                    };
                    b.ReadValue();
                }
            }

            fonte.PositionChanged += (s, ev) => Atualizar();
            fonte.ListChanged += (s, ev) => Atualizar();
            form.FormClosing += FormFechando;

            Atualizar();
        }

        public static bool Carregar(Form form, Action preencher)
        {
            try
            {
                Banco.Preparar();
                preencher();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Banco.Explicar(ex), "Banco de dados",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                form.BeginInvoke(new Action(form.Close));
                return false;
            }
        }

        public void Anterior() { fonte.MovePrevious(); }

        public void Proximo() { fonte.MoveNext(); }

        public void Novo()
        {
            fonte.AddNew();
            editando = true;
            Atualizar();
            FocarPrimeiraCaixa();
        }

        public void Alterar()
        {
            if (fonte.Count == 0) return;
            editando = true;
            Atualizar();
            FocarPrimeiraCaixa();
        }

        public void Salvar()
        {
            if (!editando) return;

            form.Validate();
            string erro = validar == null ? null : validar();
            if (!string.IsNullOrEmpty(erro))
            {
                MessageBox.Show(erro, nomeCadastro, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                fonte.EndEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Há um dado inválido no formulário.\n\n" + ex.Message,
                                nomeCadastro, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                gravar();
                fonte.ResetCurrentItem();
                editando = false;
                Atualizar();
                MessageBox.Show("Registro salvo com sucesso.", nomeCadastro,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível salvar o registro.\n\n" + ex.Message +
                                "\n\nOs dados foram recarregados do banco. Confira se o registro foi gravado.",
                                nomeCadastro, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Recarregar();
            }
        }

        private void Recarregar()
        {
            try
            {
                fonte.CancelEdit();
            }
            catch (Exception)
            {
            }

            editando = false;

            try
            {
                recarregar();
                fonte.MoveFirst();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Banco.Explicar(ex), nomeCadastro, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            fonte.ResetBindings(false);
            Atualizar();
        }

        public void Cancelar()
        {
            if (!editando) return;
            Descartar();
            Atualizar();
        }

        public void Excluir()
        {
            DataRowView atual = fonte.Current as DataRowView;
            if (atual == null || editando) return;

            string descricao = DescricaoAtual(atual);
            DialogResult r = MessageBox.Show(
                "Deseja realmente excluir " + descricao + "?", nomeCadastro,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (r != DialogResult.Yes) return;

            try
            {
                fonte.RemoveCurrent();
                gravar();
                Atualizar();
            }
            catch (Exception ex)
            {
                tabela.RejectChanges();
                fonte.ResetBindings(false);
                MessageBox.Show("Não foi possível excluir o registro.\n\n" + ex.Message,
                                nomeCadastro, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Pesquisar()
        {
            if (fonte.Count == 0 || editando) return;

            string texto = PedirTexto("Pesquisar",
                "Digite o código ou parte do " + dicaBusca + ".\n(Repita a pesquisa para ver o próximo.)",
                ultimaBusca);
            if (texto == null) return;
            texto = texto.Trim();
            if (texto.Length == 0) return;
            ultimaBusca = texto;

            int total = fonte.Count;
            for (int passo = 1; passo <= total; passo++)
            {
                int i = (fonte.Position + passo) % total;
                DataRowView linha = (DataRowView)fonte.List[i];
                if (Confere(linha, texto))
                {
                    fonte.Position = i;
                    return;
                }
            }
            MessageBox.Show("Nenhum registro encontrado para \"" + texto + "\".", nomeCadastro,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Sair() { form.Close(); }

        private void Descartar()
        {
            fonte.CancelEdit();
            tabela.RejectChanges();
            fonte.ResetCurrentItem();
            editando = false;
        }

        private void FormFechando(object sender, FormClosingEventArgs e)
        {
            if (!editando) return;
            DialogResult r = MessageBox.Show(
                "Há alterações não salvas. Deseja descartá-las e sair?", nomeCadastro,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (r == DialogResult.Yes) Descartar();
            else e.Cancel = true;
        }

        private void Atualizar()
        {
            bool tem = fonte.Count > 0;

            foreach (TextBox tb in caixas)
                tb.ReadOnly = !editando || tb == caixaChave;

            navegador.Enabled = !editando;

            btnNovo.Enabled = !editando;
            btnAlterar.Enabled = !editando && tem;
            btnExcluir.Enabled = !editando && tem;
            btnPesquisar.Enabled = !editando && tem;
            btnImprimir.Enabled = !editando && tem;
            btnAnterior.Enabled = !editando && fonte.Position > 0;
            btnProximo.Enabled = !editando && fonte.Position < fonte.Count - 1;

            btnSalvar.Enabled = editando;
            btnCancelar.Enabled = editando;
        }

        private void FocarPrimeiraCaixa()
        {
            foreach (TextBox tb in caixas)
            {
                if (!tb.ReadOnly)
                {
                    tb.Focus();
                    tb.SelectAll();
                    return;
                }
            }
        }

        private bool Confere(DataRowView linha, string texto)
        {
            if (colunaChave != null &&
                Convert.ToString(linha[colunaChave.ColumnName]) == texto)
                return true;

            CompareInfo ci = CultureInfo.CurrentCulture.CompareInfo;
            foreach (string campo in camposBusca)
            {
                string valor = Convert.ToString(linha[campo]);
                if (ci.IndexOf(valor, texto, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                    return true;
            }
            return false;
        }

        private string DescricaoAtual(DataRowView linha)
        {
            string nome = Convert.ToString(linha[camposBusca[0]]);
            return string.IsNullOrWhiteSpace(nome) ? "este registro" : "\"" + nome + "\"";
        }

        private Button Botao(string nome)
        {
            Control[] achados = form.Controls.Find(nome, true);
            if (achados.Length == 0 || !(achados[0] is Button))
                throw new InvalidOperationException("Botão '" + nome + "' não encontrado em " + form.Name);
            return (Button)achados[0];
        }

        private void ColetarCaixas(Control pai)
        {
            foreach (Control c in pai.Controls)
            {
                TextBox tb = c as TextBox;
                if (tb != null && tb.DataBindings["Text"] != null) caixas.Add(tb);
                if (c.HasChildren) ColetarCaixas(c);
            }
        }

        private string PedirTexto(string titulo, string mensagem, string padrao)
        {
            using (Form dlg = new Form())
            using (Label lbl = new Label())
            using (TextBox txt = new TextBox())
            using (Button ok = new Button())
            using (Button cancela = new Button())
            {
                dlg.Text = titulo;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new Size(380, 130);

                lbl.Text = mensagem;
                lbl.SetBounds(12, 12, 356, 40);
                txt.Text = padrao;
                txt.SetBounds(12, 60, 356, 22);
                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.SetBounds(212, 92, 75, 26);
                cancela.Text = "Cancelar";
                cancela.DialogResult = DialogResult.Cancel;
                cancela.SetBounds(293, 92, 75, 26);

                dlg.Controls.AddRange(new Control[] { lbl, txt, ok, cancela });
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancela;
                dlg.Shown += (s, ev) => { txt.Focus(); txt.SelectAll(); };

                return dlg.ShowDialog(form) == DialogResult.OK ? txt.Text : null;
            }
        }
    }

    internal static class Valida
    {
        public static string Sequencia(params Func<string>[] regras)
        {
            foreach (Func<string> regra in regras)
            {
                string erro = regra();
                if (erro != null) return erro;
            }
            return null;
        }

        public static string Obrigatorio(TextBox tb, string rotulo)
        {
            if (!string.IsNullOrWhiteSpace(tb.Text)) return null;
            tb.Focus();
            return "Informe " + rotulo + ".";
        }

        public static string Digitos(TextBox tb, string rotulo, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(tb.Text)) return null;
            int n = 0;
            foreach (char c in tb.Text) if (char.IsDigit(c)) n++;
            if (n == quantidade) return null;
            tb.Focus();
            return rotulo + " deve ter " + quantidade + " dígitos.";
        }

        public static string Uf(TextBox tb)
        {
            string t = tb.Text.Trim();
            if (t.Length == 0) return null;
            bool ok = t.Length == 2 && char.IsLetter(t[0]) && char.IsLetter(t[1]);
            if (ok) return null;
            tb.Focus();
            return "A UF deve ter 2 letras (ex.: SP).";
        }

        public static string Decimal(TextBox tb, string rotulo)
        {
            decimal v;
            if (decimal.TryParse(tb.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out v) && v >= 0)
                return null;
            tb.Focus();
            return "Informe " + rotulo + " (número maior ou igual a zero).";
        }

        public static string Inteiro(TextBox tb, string rotulo)
        {
            int v;
            if (int.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out v) && v >= 0)
                return null;
            tb.Focus();
            return "Informe " + rotulo + " (número inteiro maior ou igual a zero).";
        }
    }
}
