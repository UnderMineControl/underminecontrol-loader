using System.Drawing;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI.Controls
{
    public class ModItem : UserControl
    {
        private string _modName = "";
        public string ModName
        {
            get => _modName;
            set
            {
                _modName = value;
                Invalidate();
            }
        }

        public ModItem()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Height = 50;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var gfx = e.Graphics;

            gfx.Clear(BackColor);

            gfx.DrawString(ModName, new Font("Tahoma", 12, FontStyle.Regular), new SolidBrush(Color.Black), 10, 10);

            base.OnPaint(e);
        }
    }
}
