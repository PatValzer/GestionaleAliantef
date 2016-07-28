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
    /// Interaction logic for DataPagamento.xaml
    /// </summary>
    public partial class DataPagamento : Window
    {

        public DateTime? DataPagamentoScelta
        {
            get { return dpDataPagamento.SelectedDate; }
        }


        public DataPagamento()
        {
            InitializeComponent();
        }

        private void dpDataPagamento_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Close();
        }
    }
}
