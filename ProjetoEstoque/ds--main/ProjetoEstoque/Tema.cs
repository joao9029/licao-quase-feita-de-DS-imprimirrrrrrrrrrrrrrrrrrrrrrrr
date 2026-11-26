using System.Drawing;
using System.Windows.Forms;

namespace ProjetoEstoque
{
    internal static class Tema
    {
        public static readonly Color Fundo = Color.FromArgb(21, 88, 168);

        public static void Aplicar(Form form)
        {
            form.BackColor = Fundo;
            Pintar(form);
        }

        private static void Pintar(Control pai)
        {
            foreach (Control c in pai.Controls)
            {
                if (c is Label) c.ForeColor = Color.White;
                else if (c is MdiClient) c.BackColor = Fundo;
                if (c.HasChildren) Pintar(c);
            }
        }
    }
}
