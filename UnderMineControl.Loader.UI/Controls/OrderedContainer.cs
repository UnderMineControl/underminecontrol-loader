using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace UnderMineControl.Loader.UI.Controls
{
    public class OrderedContainer : FlowLayoutPanel
    {
        private bool _scrollToBottom = false;
        public bool ScrollToBottom
        {
            get
            {
                return _scrollToBottom;
            }
            set
            {
                _scrollToBottom = value;
            }
        }

        public OrderedContainer()
        {
            this.Resize += (a, b) =>
            {
                foreach (var control in Controls)
                    (control as Control).Width = Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth - 6;
                HorizontalScroll.Visible = false;
                if (Controls.Count > 0 && _scrollToBottom)
                    this.ScrollControlIntoView(Controls[Controls.Count - 1]);
            };
            this.ControlAdded += (a, b) =>
            {
                if (Controls.Count > 0 && _scrollToBottom)
                    this.ScrollControlIntoView(Controls[Controls.Count - 1]);
            };
            this.FlowDirection = FlowDirection.LeftToRight;
            this.WrapContents = true;
            this.AutoScroll = true;
            this.HorizontalScroll.Visible = false;
            this.Margin = new Padding(0);
            this.Padding = new Padding(0);
        }

        public void RemoveControl(Control control)
        {
            try
            {
                Controls.Remove(control);
            }
            catch { }
        }

        public void AddControl(Control control)
        {
            control.Width = Width - Margin.Left - Margin.Right - SystemInformation.VerticalScrollBarWidth - 6;
            Controls.Add(control);
        }

        public void Clear()
        {
            Controls.Clear();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        private enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            ShowScrollBar(this.Handle, (int)ScrollBarDirection.SB_HORZ, false);
            base.WndProc(ref m);
        }
    }
}
