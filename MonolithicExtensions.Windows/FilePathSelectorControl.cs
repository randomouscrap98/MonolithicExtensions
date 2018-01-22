using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MonolithicExtensions.Windows
{
    /// <summary>
    /// A simple control which allows browsing for files OR manipulating the path directly. It is very
    /// much like an HTML file input.
    /// </summary>
    public class FilePathSelector : Panel
    {
        private Button BrowseButton = new Button();
        private TextBoxWithPlaceholder PathBox = new TextBoxWithPlaceholder();

        public OpenFileDialog FileDialog { get; set; } = new OpenFileDialog();
        public FolderBrowserDialog FolderDialog { get; set; } = new FolderBrowserDialog();

        public bool UseFolderDialog { get; set; } = false;

        //public event Action<FilePathSelector, string> Text

        public override string Text
        {
            get { return PathBox.Text; }
            set { base.Text = value; PathBox.Text = value; }
        }

        public string Placeholder
        {
            get { return PathBox.Placeholder; }
            set { PathBox.Placeholder = value; }
        }

        public FilePathSelector()
        {
            this.ControlRemoved += Me_ControlRemoved;
            this.FontChanged += Me_FontChanged;
            this.Resize += Me_Resize;
            this.DragEnter += Me_DragEnter;
            this.DragDrop += Me_DragDrop;
            this.AllowDrop = true;
            BrowseButton.Click += BrowseButton_Click;
            BrowseButton.Padding = new Padding(0);
            BrowseButton.Margin = new Padding(0);
            BrowseButton.Location = new System.Drawing.Point(0, 0);
            PathBox.Padding = new Padding(0);
            PathBox.Margin = new Padding(0);
            BrowseButton.Text = "Browse";
            this.Controls.Add(BrowseButton);
            this.Controls.Add(PathBox);
            Me_Resize(null, null);
            Placeholder = "File path";
        }

        private void Me_DragDrop(object sender, DragEventArgs e)
        {
            var files = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            if(files.Count() == 1)
            {
                Text = files.First();
            }
        }

        private void Me_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            height = Math.Max(PathBox.Height, BrowseButton.Height);
            base.SetBoundsCore(x, y, width, height, specified);
        }

        /// <summary>
        /// WE make sure everything is sized correctly. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Me_Resize(object sender, EventArgs e)
        {
            BrowseButton.Height = PathBox.Height + 2;
            PathBox.Location = new System.Drawing.Point(BrowseButton.Width,1);
            PathBox.Width = Width - BrowseButton.Width;
        }

        /// <summary>
        /// Ensure the font for all our controls stays the same.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Me_FontChanged(object sender, EventArgs e)
        {
            BrowseButton.Font = this.Font;
            PathBox.Font = this.Font;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (UseFolderDialog)
            {
                if (FolderDialog == null)
                    FolderDialog = new FolderBrowserDialog();

                var result = FolderDialog.ShowDialog();

                if (result == DialogResult.OK)
                    Text = FolderDialog.SelectedPath;
            }
            else
            {
                if (FileDialog == null)
                    FileDialog = new OpenFileDialog();

                var result = FileDialog.ShowDialog();

                if (result == DialogResult.OK)
                    Text = FileDialog.FileName;
            }
        }

        /// <summary>
        /// When removing controls, we must always make sure that our placeholder control exists
        /// </summary>
        /// <param name="e"></param>
        private void Me_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (!this.Controls.Contains(BrowseButton))
                this.Controls.Add(BrowseButton);
            if (!this.Controls.Contains(PathBox))
                this.Controls.Add(PathBox);
        }
    }
}
