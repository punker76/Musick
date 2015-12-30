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
    /// Interaction logic for MusickError.xaml
    /// </summary>
    public partial class MusickError
    {
        public string errorMessage { get; set; }
        public MusickError()
        {
            InitializeComponent();
            this.lblError.Content = errorMessage;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
