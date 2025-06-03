using System.Drawing;
using System.Windows.Forms;

namespace TodoApp
{
    public static class DarkTheme
    {
        public static void Apply(Control control)
        {
            control.BackColor = Color.FromArgb(30, 30, 30);
            control.ForeColor = Color.White;

            foreach (Control c in control.Controls)
            {
                Apply(c);
            }
        }
    }
}
