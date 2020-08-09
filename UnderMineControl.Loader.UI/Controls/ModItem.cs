using System.Drawing;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI.Controls
{
    using Core.Models;

    public class ModItem : UserControl
    {
        private Mod _mod;

        public Mod Mod { get => _mod; set { _mod = value; Invalidate(); } }

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

            if (_mod == null)
            {
                base.OnPaint(e);
                return;
            }

            //gfx.DrawString(ModName, new Font("Tahoma", 12, FontStyle.Regular), new SolidBrush(Color.Black), 10, 10);

            base.OnPaint(e);
        }
    }
}
