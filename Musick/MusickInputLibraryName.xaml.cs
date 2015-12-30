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
using System.Text.RegularExpressions;

namespace Musick
{
    /// <summary>
    /// Interaction logic for MusickInputLibraryName.xaml
    /// </summary>
    public partial class MusickInputLibraryName
    {
        public MusickInputLibraryName()
        {
            InitializeComponent();
        }

        private void txtLibraryName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(LibraryNameCheck(txtLibraryName.Text))
                {
                    this.DialogResult = true;
                }
                else
                {
                    txtLibraryName.Clear();
                    txtLibraryName.Focus();
                    lblLibNameError.Visibility = Visibility.Visible;
                }
            }
        }
        private bool LibraryNameCheck(string libName)
        {
            {
                if (libName.Length > 0 && libName.Length < 30)
                {
                    Regex regex = new Regex("^[a-z0-9_-]+$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(libName))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = Owner.Left;
            this.Top = Owner.Top + Owner.Height;
        }
    }
}
