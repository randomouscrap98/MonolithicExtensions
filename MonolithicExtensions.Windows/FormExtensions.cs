using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace MonolithicExtensions.Windows
{
    /// <summary>
    /// Extensions to various Windows Forms Controls
    /// </summary>
    public static class FormControlExtensions
    {
        /// <summary>
        /// Allows retrieval of textbox text as a guid object.
        /// </summary>
        /// <param name="GuidTextbox"></param>
        /// <returns>The Guid, or Nothing on failure</returns>
        public static Guid TextAsGuid(this TextBox GuidTextbox)
        {
            try
            {
                return Guid.Parse(GuidTextbox.Text);
            }
            catch //(Exception ex)
            {
                return default(Guid);
            }
        }

        /// <summary>
        /// Allows the retrieval of textbox text as a version object
        /// </summary>
        /// <param name="GuidTextbox"></param>
        /// <returns>The Version, or Nothing on failure</returns>
        public static Version TextAsVersion(this TextBox GuidTextbox)
        {
            try
            {
                return Version.Parse(GuidTextbox.Text);
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves an object stored in the Tag field from the given control and stores it in Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TheControl"></param>
        /// <param name="Result"></param>
        /// <returns>Whether or not the Tag was able to be stored in Result</returns>
        public static bool TryGetTagAsObject<T>(this Control TheControl, ref T Result)
        {
            if (TheControl != null && TheControl.Tag is T)
            {
                Result = (T)TheControl.Tag;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves an object stored in the Tag field from the given TreeNode and stores it in Result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Node"></param>
        /// <param name="Result"></param>
        /// <returns>Whether or not the Tag was able to be stored in Result</returns>
        public static bool TryGetTagAsObject<T>(this TreeNode Node, ref T Result)
        {
            if (Node != null && Node.Tag is T)
            {
                Result = (T)Node.Tag;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns all the tree nodes from any level for which the given test yeilds true. This calls the recursive function
        /// "FindTreeNodes", so it may get stuck if there is a circular dependency.
        /// </summary>
        /// <param name="Tree"></param>
        /// <param name="Test"></param>
        /// <returns></returns>
        public static List<TreeNode> FindTreeNodes(this TreeView Tree, Func<TreeNode, bool> Test)
        {

            List<TreeNode> allMatchingNodes = new List<TreeNode>();

            foreach (TreeNode node in Tree.Nodes)
            {
                allMatchingNodes.AddRange(node.FindTreeNodes(Test));
            }

            return allMatchingNodes;
        }

        /// <summary>
        /// Returns all the tree nodes from any level for which the given test yields true. This is a recursive function 
        /// which has the potential to never return IF there is a circular dependency (which there shouldn't be)
        /// </summary>
        /// <param name="BaseNode"></param>
        /// <param name="Test"></param>
        /// <returns></returns>
        public static List<TreeNode> FindTreeNodes(this TreeNode BaseNode, Func<TreeNode, bool> Test)
        {
            List<TreeNode> foundNodes = new List<TreeNode>();

            if (Test(BaseNode))
                foundNodes.Add(BaseNode);
            //The basic test

            foreach (TreeNode node in BaseNode.Nodes)
            {
                foundNodes.AddRange(FindTreeNodes(node, Test));
                //Then add any nodes found deeper
            }

            return foundNodes;
        }

        //Find treenodes based on the given tag
        public static List<TreeNode> FindTreeNodesByTag<T>(this TreeView Tree, T TagValue)
        {
            return Tree.FindTreeNodes((TreeNode node) => node.Tag is T && TagValue.Equals(node.Tag));
            //CType(node.Tag, T).Equals(TagValue))
        }

        //Find treenodes based on the given tag
        public static List<TreeNode> FindTreeNodesByTag<T>(this TreeNode BaseNode, T TagValue)
        {
            return BaseNode.FindTreeNodes((TreeNode node) => node.Tag is T && TagValue.Equals(node.Tag));
            //CType(node.Tag, T).Equals(TagValue))
        }

        /// <summary>
        /// Create a label that will fit nicely within a table row
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Label CreateTableLabel(string text, bool leftAlign = false)
        {
            var label = new Label();

            label.Text = text;
            label.Dock = DockStyle.Fill;
            label.AutoSize = false;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            if (leftAlign)
                label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            return label;
        }

        /// <summary>
        /// Attempt to convert the selected value in the combobox into the given enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Box"></param>
        /// <returns></returns>
        public static bool TryGetSelectedEnum<T>(this ComboBox Box, ref T Result) where T : struct, IConvertible
        {
            if (Box.SelectedItem == null)
                return false;
            return Enum.TryParse(Box.SelectedItem.ToString(), out Result);
        }

        /// <summary>
        /// Given a combobox, this will fill the combobox with all the values for a given enumeration
        /// </summary>
        /// <param name="Box"></param>
        public static void FillWithEnumeration(this ComboBox Box, Type Enumeration, IEnumerable<string> Extras = null)
        {
            Box.Items.Clear();

            if (Extras == null)
                Extras = new List<string>();

            foreach (var type in Extras.Union(Enum.GetNames(Enumeration)))
            {
                Box.Items.Add(type);
            }
        }

        /// <summary>
        /// Searches all children to find the deepest control with focus.
        /// </summary>
        /// <param name="ctr"></param>
        /// <returns></returns>
        /// <remarks>Found online at http://stackoverflow.com/a/9634362/1066474 By MarkJ</remarks>
        public static Control FindFocussedControl(Control ctr)
        {
            ContainerControl container = ctr as ContainerControl;
            while ((container != null))
            {
                ctr = container.ActiveControl;
                container = ctr as ContainerControl;
            }
            return ctr;
        }

        /// <summary>
        /// Searches all child controls (including the given control) and returns any with the given name.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<Control> FindControlsByName(this Control parentControl, string name)
        {
            List<Control> controls = new List<Control>();

            if (parentControl.Name == name)
                controls.Add(parentControl);

            foreach (Control control in parentControl.Controls)
            {
                controls.AddRange(control.FindControlsByName(name));
            }

            return controls;
        }

        /// <summary>
        /// Search ALL open forms for any control with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<Control> FindAllControlsByName(string name)
        {
            return Application.OpenForms.Cast<Control>().SelectMany(x => x.FindControlsByName(name)).ToList();
        }

        public enum ControlStatus
        {
            Normal,
            Error,
            Warning
        }

        /// <summary>
        /// A simple way to set a visual status on controls (useful for showing an error has occurred on input)
        /// </summary>
        /// <param name="SomeControl"></param>
        /// <param name="Status"></param>
        public static void SetVisualStatus(this Control SomeControl, ControlStatus Status)
        {
            //This uses the control's Tag field to store the original backcolor of the control.
            if (SomeControl.Tag == null)
            {
                SomeControl.Tag = SomeControl.BackColor;
            }

            if (Status == ControlStatus.Error)
            {
                SomeControl.BackColor = Color.LightSalmon;
            }
            else if (Status == ControlStatus.Warning)
            {
                SomeControl.BackColor = Color.LemonChiffon;
            }
            else
            {
                if (SomeControl.Tag is Color)
                {
                    SomeControl.BackColor = (Color)SomeControl.Tag;
                }
                else
                {
                    //Logger.Warn("Could not find control's original color when setting status. Using White instead.")
                    SomeControl.BackColor = Color.White;
                }
            }
        }

        public static void FillWithList<T>(this ListView view, IEnumerable<T> items, Func<T, string> displayFunction = null)
        {
            if (displayFunction == null)
                displayFunction = x => x.ToString();

            view.Clear();
            view.View = View.Details;
            view.Scrollable = true;
            view.HeaderStyle = ColumnHeaderStyle.None;
            view.FullRowSelect = true;

            var header = new ColumnHeader();
            header.Text = "";
            header.Name = "column" + DateTime.Now.Ticks;
            view.Columns.Add(header);

            foreach (T item in items)
            {
                var viewItem = new ListViewItem(displayFunction(item));
                viewItem.Tag = item;
                view.Items.Add(viewItem);
            }

            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        //public static void FillWithList<T>(this ComboBox box, IEnumerable<T> items, Func<T, string> displayFunction = null)
        //{
        //    if (displayFunction == null)
        //        displayFunction = x => x.ToString();

        //    box.Items.Clear();
        //    foreach(T item in items)
        //    {
        //        var comboboxItem = new item
        //    }
        //}

        public static void SafeAppend(this TextBox textbox, string message)
        {
            if (textbox.InvokeRequired)
            {
                textbox.BeginInvoke(new Action(() => textbox.AppendText(message)));
            }
            else
            {
                textbox.AppendText(message);
            }
        }

        /// <summary>
        /// Functions taken from the windows API to augment form services
        /// </summary>
        public class WinAPI
        {
            //Constants for types of messages and commands
            public const int WM_SYSCOMMAND = 0x112;
            public const int SC_CONTEXTHELP = 0xf180;

            /// <summary>
            /// Allows you to send direct messages to the Windows system to enable various states/etc.
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="msg"></param>
            /// <param name="wp"></param>
            /// <param name="lp"></param>
            /// <returns></returns>
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

            /// <summary>
            /// Allows you to invoke the Help state so that the next thing you click will show the help text
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="lp"></param>
            /// <returns></returns>
            public static IntPtr SendHelpMessage(IntPtr hWnd, IntPtr lp)
            {
                return SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)SC_CONTEXTHELP, lp);
            }
        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
