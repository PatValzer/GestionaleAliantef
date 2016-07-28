using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for GeneraMastrino.xaml
    /// </summary>
    public partial class GeneraMastrino : Window
    {
        Noleggio _nol = new Noleggio();
        public GeneraMastrino(Noleggio nol)
        {
            InitializeComponent();
            _nol = nol;
        }


        private void btnOKMastrino_Click(object sender, RoutedEventArgs e)
        {
            GestioneReport.MostraPdf(GestioneReport.GeneraMastrino(_nol, dpDal.SelectedDate, dpAL.SelectedDate, (bool)cbxTotaleCliente.IsChecked));
            this.Close();
        }

        private void btnCancellaMastrino_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

  
    }
}
