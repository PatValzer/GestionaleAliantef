using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();

            var count = new AlianteLinqDataContext().Noleggios.Where(nol => (nol.NoleggioChiuso != 1 || nol.NoleggioChiuso == null) && nol.dataProssimaProroga <= DateTime.Today).Count();
            if (count > 0)
                MessageBox.Show("Ci sono " + count.ToString() + " noleggi da prorogare");
            RicaricaListaNoleggi();
        }

        private void dgNoleggi_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var noleggio = e.Row.Item;
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var newW = new ClienteGestione();
            newW.ShowDialog(); // works
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            var newW = new BancaGestione();
            newW.ShowDialog(); // works
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            var newW = new GestioneNoleggio();
            InserisciModificaNoleggio(newW);
        }

        private void AButton_Click_1(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var id = btn.CommandParameter.ToString();
            var guid = new Guid(id);
            var newW = new GestioneNoleggio(guid);
            InserisciModificaNoleggio(newW);
        }

        private void InserisciModificaNoleggio(GestioneNoleggio newW)
        {
            newW.ShowDialog();
            RicaricaListaNoleggi();
        }

        private void RicaricaListaNoleggi()
        {
            List<vwNoleggi> noleggi = new List<vwNoleggi>();
            var ctx = new AlianteLinqDataContext();

            if (cbxChiuso != null)
                if ((bool)cbxChiuso.IsChecked)
                    foreach (var nol in ctx.Noleggios)
                    {
                        if (nol.NoleggioChiuso == 1 && !(nol.Pagamentos.Where(s => s.Pagato != 1).Count() > 0 || nol.Interventos.Where(s => s.Pagato != 1).Count() > 0 || nol.Prorogas.Where(s => s.Pagato != 1).Count() == 0))
                            noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio));
                    }

            if (cbxChiusoDaPagare != null)
                if ((bool)cbxChiusoDaPagare.IsChecked)
                {
                    foreach (var nol in ctx.Noleggios)
                    {
                        if (nol.NoleggioChiuso == 1 && (nol.Pagamentos.Where(s => s.Pagato != 1).Count() > 0 || nol.Interventos.Where(s => s.Pagato != 1).Count() > 0 || nol.Prorogas.Where(s => s.Pagato != 1).Count() == 0))
                            noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio));
                    }
                }

            if (cbxAperto != null)
                if ((bool)cbxAperto.IsChecked)
                    foreach (var nol in ctx.Noleggios)
                    {
                        if (nol.NoleggioChiuso != 1 && nol.dataProssimaProroga > DateTime.Today && !(nol.Pagamentos.Where(s => s.Pagato != 1 || s.Pagato == null).Count() > 0 || nol.Interventos.Where(s => s.Pagato != 1).Count() > 0 || nol.Prorogas.Where(s => s.Pagato != 1 || s.Pagato == null).Count() == 0))
                            noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio));
                    }

            if (cbxDaProrogare != null)
                if ((bool)cbxDaProrogare.IsChecked)
                    foreach (var nol in ctx.Noleggios)
                    {
                        if (nol.NoleggioChiuso != 1 && nol.dataProssimaProroga <= DateTime.Today)
                            noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio));
                    }

            if (cbxApertoNonPagato != null)
                if ((bool)cbxApertoNonPagato.IsChecked)
                    foreach (var nol in ctx.Noleggios)
                    {
                        if (nol.NoleggioChiuso != 1 && (nol.Pagamentos.Where(s => s.Pagato != 1).Count() > 0 || nol.Interventos.Where(s => s.Pagato != 1).Count() > 0 || nol.Prorogas.Where(s => s.Pagato != 1).Count() == 0))
                            if ((bool)cbxDaProrogare.IsChecked)
                                noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio && s.dataProssimaProroga > DateTime.Today));
                            else
                                noleggi.AddRange(ctx.vwNoleggis.Where(s => s.idNoleggio == nol.idNoleggio));
                    }

            if (txtRicerca != null)
                if (!string.IsNullOrEmpty(txtRicerca.Text))
                    dgNoleggi.ItemsSource = noleggi.OrderByDescending(s => s.dataFineNoleggio).Where(s => s.RagioneSociale.Contains(txtRicerca.Text) || s.Indirizzo.Contains(txtRicerca.Text));
                else
                    dgNoleggi.ItemsSource = noleggi.OrderByDescending(s => s.dataFineNoleggio);
        }

        private void AButton_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Vuoi Eliminare questo noleggio", "Confirmation", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var btn = (Button)sender;
                var id = btn.CommandParameter.ToString();
                var guid = new Guid(id);
                UtilityNoleggio.EliminaNoleggio(guid);
                RicaricaListaNoleggi();
            }
        }

        private void cmbCliente_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void txtRicerca_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnumerableQuery<vwNoleggi> noleggi = (EnumerableQuery<vwNoleggi>)dgNoleggi.ItemsSource.AsQueryable();
            if (txtRicerca != null)
                if (!string.IsNullOrEmpty(txtRicerca.Text))
                    dgNoleggi.ItemsSource = noleggi.OrderByDescending(s => s.dataFineNoleggio).Where(s => s.RagioneSociale.ToUpper().Contains(txtRicerca.Text.ToUpper()) || s.Indirizzo.ToUpper().Contains(txtRicerca.Text.ToUpper()));
                else
                    dgNoleggi.ItemsSource = noleggi.OrderByDescending(s => s.dataFineNoleggio);
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {

            var newW = new Mastrino();
            newW.WindowState = WindowState.Maximized;
            newW.ShowDialog();
        }

        private void cbxDaProrogare_Copy_Checked(object sender, RoutedEventArgs e)
        {
            RicaricaListaNoleggi();
        }



    }
}
