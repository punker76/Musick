using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro;
using System.IO;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickInputLibraryLocation.xaml
    /// </summary>
    public partial class MusickInputLibraryLocation
    {
        public MusickInputLibraryLocation()
        {
            InitializeComponent();
            // Fill the treeview up with folders.
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }            
            this.Left = SystemParameters.PrimaryScreenWidth / 2 - this.Width / 2;
            this.Top = SystemParameters.PrimaryScreenHeight / 2 + 120;
        }

        //Sets the label when the selected item has changed.
        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Sets the folderbrowse textbox to the path.
            lblSelectedFolder.Content = ((TreeViewItem)e.NewValue).Tag.ToString();
            btnAccept.Visibility = Visibility.Visible;
        }

            void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }
        private object dummyNode = null;

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if(lblSelectedFolder.Content!=null)
            {
                this.DialogResult = true;
            }           
        }
    }
}
