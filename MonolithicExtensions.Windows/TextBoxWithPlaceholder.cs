using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace MonolithicExtensions.Windows
{
    public class TextBoxWithPlaceholder : System.Windows.Forms.TextBox
    {
        /// <summary>
        /// The control which holds the placeholder text
        /// </summary>
        /// <returns></returns>
        private Label PlaceholderControl { get; set; } = new Label();

        public TextBoxWithPlaceholder()
        {
            LostFocus += Me_LostFocus;
            GotFocus += Me_GotFocus;
            ControlRemoved += Me_ControlRemoved;
            FontChanged += Me_FontChanged;
            PlaceholderControl.ForeColor = System.Drawing.Color.Gray;
            PlaceholderControl.BackColor = System.Drawing.Color.Transparent;
            PlaceholderControl.Padding = new Padding(0);
            PlaceholderControl.Margin = new Padding(0);
            //-5, 0, 0, 0)
            PlaceholderControl.Enabled = false;
            PlaceholderControl.Dock = DockStyle.Fill;
            this.Controls.Add(PlaceholderControl);
            this.TextChanged += (e, s) => RefreshPlaceholderVisibility();
        }

        /// <summary>
        /// Set or get the placeholder text for this textbox
        /// </summary>
        /// <returns></returns>
        public string Placeholder
        {
            get { return PlaceholderControl.Text; }
            set { PlaceholderControl.Text = value; }
        }

        /// <summary>
        /// Get or set the placeholder text color for this textbox
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Color PlaceholderColor
        {
            get { return PlaceholderControl.ForeColor; }
            set { PlaceholderControl.ForeColor = value; }
        }

        /// <summary>
        /// Refresh the visibility of the placeholder. SHould occur on every focus and text change
        /// </summary>
        protected void RefreshPlaceholderVisibility()
        {
            PlaceholderControl.Visible = (!this.Focused && string.IsNullOrEmpty(this.Text));
        }

        /// <summary>
        /// The placeholder font must keep the same font as the textbox, so we intercept the font changing event and sync the fonts up
        /// </summary>
        /// <param name="e"></param>
        private void Me_FontChanged(object sender, EventArgs e)
        {
            PlaceholderControl.Font = this.Font;
        }

        /// <summary>
        /// When removing controls, we must always make sure that our placeholder control exists
        /// </summary>
        /// <param name="e"></param>
        private void Me_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (!this.Controls.Contains(PlaceholderControl))
                this.Controls.Add(PlaceholderControl);
        }

        /// <summary>
        /// Changing focus must refresh the placeholder visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Me_GotFocus(object sender, EventArgs args)
        {
            RefreshPlaceholderVisibility();
        }

        /// <summary>
        /// Changing focus must refresh the placeholder visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Me_LostFocus(object sender, EventArgs args)
        {
            RefreshPlaceholderVisibility();
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
