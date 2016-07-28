using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for Cliente.xaml
    /// </summary>
    public partial class ClienteGestione : Window
    {
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        Cliente _cliente;
        public ClienteGestione()
        {
            InitializeComponent();
            CaricaComboCliente();
            NuovoCliente();
        }

        private void NuovoCliente()
        {
            RicaricaContext();
            _cliente = new Cliente();
            this.DataContext = _cliente;
            _cliente.IdCliente = Guid.NewGuid();
            _cliente.Indirizzo = new Indirizzo();
            _cliente.Indirizzo.IdIndirizzo = Guid.NewGuid();
            cmbBanca.SelectedIndex = -1;
            cmbBanca.ItemsSource = _ctx.Bancas.OrderBy(s => s.NomeBanca);
            cmbBanca.DisplayMemberPath = "NomeBanca";

            if (_cliente != null)
                if (_cliente.Banca != null)
                    cmbBanca.SelectedItem = _cliente.Banca;

            gbIndirizzo.DataContext = _cliente.Indirizzo;
            _ctx.Clientes.InsertOnSubmit(_cliente);

            cmbCliente.ItemsSource = new AlianteLinqDataContext().Clientes.OrderBy(s => s.RagioneSociale);
            cmbCliente.DisplayMemberPath = "RagioneSociale";

        }





        private void CaricaComboCliente()
        {
            cmbCliente.ItemsSource = new AlianteLinqDataContext().Clientes.OrderBy(s => s.RagioneSociale);
            cmbCliente.DisplayMemberPath = "RagioneSociale";
            if (_cliente != null)
            {
                cmbCliente.SelectedItem = _cliente;
                if (_cliente.Banca != null)
                    cmbBanca.SelectedItem = _cliente.Banca;
            }

        }

        private void CaricaDatiCliente(Cliente cl)
        {



            var changes = _ctx.GetChangeSet();

            if (cl != null)
            {
                // Delete the insertions
                foreach (Cliente insertion in changes.Inserts.Where(s => s.GetType() == cl.GetType()))
                    _ctx.Clientes.DeleteOnSubmit(insertion);


                if (cl.Indirizzo == null)
                {
                    cl.Indirizzo = new Indirizzo();
                    cl.Indirizzo.IdIndirizzo = Guid.NewGuid();
                }


                if (cl.Banca == null)
                {
                    cmbBanca.SelectedIndex = -1;
                }
                else
                    foreach (var item in cmbBanca.Items)
                    {
                        if (((Banca)item).idBanca == cl.Banca.idBanca)
                            cmbBanca.SelectedItem = item;
                    }


                _cliente = cl;
            }
            else
            {

                NuovoCliente();
            }
            this.DataContext = _cliente;
            gbIndirizzo.DataContext = _cliente.Indirizzo;
        }


        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_cliente.RagioneSociale))
            {
                //if (cmbBanca.SelectedIndex != -1)
                //    _ctx.ExecuteCommand("update Cliente set idBanca = '" + ((Banca)cmbBanca.SelectedItem).idBanca + "' where idCliente = '" + _cliente.IdCliente + "'");

                _ctx.SubmitChanges();
                RicaricaContext();
                CaricaComboCliente();
            }
        }

        private void RicaricaContext()
        {
            Guid id = Guid.Empty;
            if (_cliente != null)
                id = _cliente.IdCliente;
            _ctx = new AlianteLinqDataContext();
            if (id != Guid.Empty)
                if (_ctx.Clientes.Where(s => s.IdCliente == id).Count() > 0)
                    _cliente = _ctx.Clientes.Where(s => s.IdCliente == id).First();
        }



        private void cmbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem != null)
            {
                var id = ((Cliente)(((ComboBox)sender).SelectedItem)).IdCliente;
                var cliente = _ctx.Clientes.Where(s => s.IdCliente == id).First();
                CaricaDatiCliente(cliente);
            }

        }

        private void btnElimina_Click(object sender, RoutedEventArgs e)
        {
            if (_cliente.Indirizzo != null)
                _ctx.Indirizzos.DeleteAllOnSubmit(_ctx.Indirizzos.Where(s => s.IdIndirizzo == _cliente.Indirizzo.IdIndirizzo));
            _ctx.Clientes.DeleteOnSubmit(_ctx.Clientes.Where(s => s.IdCliente == _cliente.IdCliente).First());
            try
            {
                _ctx.SubmitChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossibile eliminare il cliente, perchè associato a un noleggio");
                RicaricaContext();
            }
            NuovoCliente();
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            NuovoCliente();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var newW = new BancaGestione();
            newW.ShowDialog(); // works
            _cliente.Banca =  _ctx.Bancas.Where(s=> s.idBanca == newW._banca.idBanca).First();

            cmbBanca.ItemsSource = _ctx.Bancas.OrderBy(s=> s.NomeBanca);
            cmbBanca.DisplayMemberPath = "NomeBanca";
            cmbBanca.SelectionChanged -= cmbBanca_SelectionChanged;
            cmbBanca.SelectedIndex = -1;
            cmbBanca.SelectedItem = _cliente.Banca;
            cmbBanca.SelectionChanged += cmbBanca_SelectionChanged;
        }

        private void cmbBanca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _cliente.Banca = (Banca)cmbBanca.SelectedItem;
        }



    }
}
