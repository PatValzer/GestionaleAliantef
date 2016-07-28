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
    /// Interaction logic for BancaGestione.xaml
    /// </summary>
    public partial class BancaGestione : Window
    {

        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        public Banca _banca;

        public BancaGestione()
        {
            InitializeComponent();
            NuovaBanca();
            CaricaComboBanche();
        }

        private void cmbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            CaricaDatiBanca((Banca)(((ComboBox)sender).SelectedItem));
        }

        private void CaricaDatiBanca(Banca cl)
        {

            var changes = _ctx.GetChangeSet();

            if (cl != null)
            {
                // Delete the insertions
                foreach (Banca insertion in changes.Inserts.Where(s => s.GetType() == cl.GetType()))
                    _ctx.Bancas.DeleteOnSubmit(insertion);
                _banca = _ctx.Bancas.Where(s => s.idBanca == cl.idBanca).First();
                this.DataContext = _banca;
            }
            else
            {

                NuovaBanca();
            }
        }

        private void NuovaBanca()
        {
            _banca = new Banca();
            this.DataContext = _banca;
            _banca.idBanca = Guid.NewGuid();
            _ctx.Bancas.InsertOnSubmit(_banca);
        }

        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            //_banca.ABI = txtABI.Text;
            //_banca.CAB = txtCAB.Text;
            if (txtCodiceIban.Text == "")
                _banca.IBAN = "";
            if (txtABI.Text == "")
                _banca.ABI = "";
            if (txtCAB.Text == "")
                _banca.CAB = "";
            //_banca.NomeBanca = txtNome.Text;
            _ctx.SubmitChanges();
            CaricaComboBanche();
        }

        private void CaricaComboBanche()
        {
            cmbBanca.ItemsSource = _ctx.Bancas.OrderBy(s => s.NomeBanca);
            cmbBanca.DisplayMemberPath = "NomeBanca";
            if (_banca != null)
                cmbBanca.SelectedItem = _banca;
        }

        private void btnElimina_Click(object sender, RoutedEventArgs e)
        {

            _ctx.Bancas.DeleteOnSubmit(_ctx.Bancas.Where(s => s.idBanca == _banca.idBanca).First());
            _ctx.SubmitChanges();
            CaricaComboBanche();
        }

        private void imgAggiungiBanca_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaricaDatiBanca(null);
        }
    }
}
