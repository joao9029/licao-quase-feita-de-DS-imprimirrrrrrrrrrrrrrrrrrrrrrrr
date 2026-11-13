using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace ProjetoEstoque
{
    internal class Impressao
    {
        private readonly string titulo;
        private readonly DataTable tabela;
        private readonly string[] campos;
        private readonly string[] cabecalhos;
        private readonly float[] larguras;

        private int linhaAtual;
        private int numeroPagina;

        private Impressao(string titulo, DataTable tabela, string[] campos,
                          string[] cabecalhos, float[] larguras)
        {
            this.titulo = titulo;
            this.tabela = tabela;
            this.campos = campos;
            this.cabecalhos = cabecalhos;
            this.larguras = larguras;
        }

        public static void Imprimir(string titulo, DataTable tabela, string[] campos,
                                    string[] cabecalhos, float[] larguras)
        {
            if (tabela == null || tabela.Rows.Count == 0)
            {
                MessageBox.Show("Não há registros para imprimir.", titulo,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Impressao imp = new Impressao(titulo, tabela, campos, cabecalhos, larguras);

            using (PrintDocument doc = new PrintDocument())
            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                doc.DocumentName = titulo;
                doc.DefaultPageSettings.Landscape = true;
                doc.BeginPrint += (s, e) => { imp.linhaAtual = 0; imp.numeroPagina = 0; };
                doc.PrintPage += imp.ImprimirPagina;

                preview.Document = doc;
                preview.WindowState = FormWindowState.Maximized;
                preview.ShowDialog();
            }
        }

        private void ImprimirPagina(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle area = e.MarginBounds;
            numeroPagina++;

            using (Font fTitulo = new Font("Arial", 16, FontStyle.Bold))
            using (Font fCabecalho = new Font("Arial", 10, FontStyle.Bold))
            using (Font fTexto = new Font("Arial", 9))
            using (Font fRodape = new Font("Arial", 8))
            using (Pen linha = new Pen(Color.Black, 1))
            using (StringFormat sf = new StringFormat())
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;
                sf.LineAlignment = StringAlignment.Center;

                float y = area.Top;

                g.DrawString(titulo, fTitulo, Brushes.Black, area.Left, y);
                y += fTitulo.GetHeight(g) + 10;

                float soma = 0;
                foreach (float l in larguras) soma += l;
                float[] x = new float[larguras.Length + 1];
                x[0] = area.Left;
                for (int i = 0; i < larguras.Length; i++)
                    x[i + 1] = x[i] + area.Width * larguras[i] / soma;

                float altCabecalho = fCabecalho.GetHeight(g) + 8;
                float altLinha = fTexto.GetHeight(g) + 6;
                float altRodape = fRodape.GetHeight(g) + 6;

                for (int i = 0; i < cabecalhos.Length; i++)
                {
                    RectangleF r = new RectangleF(x[i] + 2, y, x[i + 1] - x[i] - 4, altCabecalho);
                    g.DrawString(cabecalhos[i], fCabecalho, Brushes.Black, r, sf);
                }
                y += altCabecalho;
                g.DrawLine(linha, area.Left, y, area.Right, y);

                float limite = area.Bottom - altRodape;
                while (linhaAtual < tabela.Rows.Count && y + altLinha <= limite)
                {
                    DataRow row = tabela.Rows[linhaAtual];
                    for (int i = 0; i < campos.Length; i++)
                    {
                        object valor = row[campos[i]];
                        string texto = valor == DBNull.Value ? ""
                                       : (valor is decimal ? ((decimal)valor).ToString("N2")
                                                           : Convert.ToString(valor));
                        RectangleF r = new RectangleF(x[i] + 2, y, x[i + 1] - x[i] - 4, altLinha);
                        g.DrawString(texto, fTexto, Brushes.Black, r, sf);
                    }
                    y += altLinha;
                    linhaAtual++;
                }

                g.DrawLine(linha, area.Left, limite, area.Right, limite);
                g.DrawString("Emitido em " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                             fRodape, Brushes.Black, area.Left, limite + 3);
                string total = "Total de registros: " + tabela.Rows.Count;
                SizeF tamTotal = g.MeasureString(total, fRodape);
                g.DrawString(total, fRodape, Brushes.Black, area.Left + (area.Width - tamTotal.Width) / 2, limite + 3);
                string pag = "Página " + numeroPagina;
                SizeF tam = g.MeasureString(pag, fRodape);
                g.DrawString(pag, fRodape, Brushes.Black, area.Right - tam.Width, limite + 3);
            }

            e.HasMorePages = linhaAtual < tabela.Rows.Count;
        }
    }
}
