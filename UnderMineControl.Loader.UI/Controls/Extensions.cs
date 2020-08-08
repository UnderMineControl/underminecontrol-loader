using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace UnderMineControl.Loader.UI.Controls
{
    public static class Extensions
    {
        public static StripBuilder Build(this MenuStrip strip)
        {
            return new StripBuilder(strip);
        }


        public class StripBuilder
        {
            public object Parent { get; private set; }
            public ToolStripItemCollection Collection { get; private set; }
            public bool IsMenuStrip { get; private set; }

            public StripBuilder(MenuStrip parent)
            {
                Parent = parent;
                IsMenuStrip = true;
                Collection = parent.Items;
            }

            public StripBuilder(ToolStripMenuItem item)
            {
                Parent = item;
                IsMenuStrip = false;
                Collection = item.DropDownItems;
            }

            public StripBuilder Add(ToolStripItem item, Action<ToolStripItem> config = null)
            {
                config?.Invoke(item);
                Collection.Add(item);
                return this;
            }

            public StripBuilder Add<T>(T item, Action<T> config = null) where T : ToolStripItem
            {
                return Add((ToolStripItem)item, (c) => config?.Invoke((T)c));
            }

            public StripBuilder AddButton(string text, Action<object, EventArgs, ToolStripButton> onClick, string toolTip = null, Action<ToolStripButton> config = null)
            {
                var item = new ToolStripButton(text);
                item.ToolTipText = toolTip;
                item.Click += (e, s) => onClick(e, s, item);
                return Add(item, config);
            }

            public StripBuilder AddButton(string text, Action<EventArgs> onClick, string toolTip = null, Action<ToolStripButton> config = null)
            {
                return AddButton(text, (e, s, b) => onClick(s), toolTip, config);
            }

            public StripBuilder AddButton(string text, Action onClick, string toolTip = null, Action<ToolStripButton> config = null)
            {
                return AddButton(text, (e, s, b) => onClick(), toolTip, config);
            }

            public StripBuilder AddButton(string text, Action<ToolStripButton> onClick, string toolTip = null, Action<ToolStripButton> config = null)
            {
                return AddButton(text, (e, s, b) => onClick(b), toolTip, config);
            }

            public StripBuilder AddSeperator(Action<ToolStripSeparator> config = null)
            {
                return Add(new ToolStripSeparator(), config);
            }

            public StripBuilder AddTextBox(Action<ToolStripTextBox> config = null)
            {
                return Add(new ToolStripTextBox(), config);
            }

            public StripBuilder AddSubMenu(string text, Action<StripBuilder> config)
            {
                var item = new ToolStripMenuItem(text);
                Collection.Add(item);

                var strip = new StripBuilder(item);
                config?.Invoke(strip);

                return this;
            }
        }
    }
}
