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
                this.DialogResult = true;
            }
        }
    }
}
