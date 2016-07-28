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
using System.Windows.Threading;

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for NuovaProroga.xaml
    /// </summary>
    public partial class PagamentiIntervento : Window
    {


       
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        Intervento _intervento = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nol">Il Noleggio da prorogare</param>
  
        /// <summary>
        /// Inizializza i campi
        /// </summary>
        /// <param name="idProroga"></param>
        /// <param name="nol"></param>
        public PagamentiIntervento(Guid idIntervento)
        {
            InitializeComponent();
            var intervento  = _ctx.Interventos.Where(s => s.idIntervento == idIntervento).First();

            _intervento = intervento;

            decimal totalePagamenti = TotalePagamenti();
            lblTotale.Content = _intervento.ImportoTotaleIvato - totalePagamenti;
            dgPagamenti.ItemsSource = _ctx.PagamentoInterventos.Where(s => s.idIntervento == _intervento.idIntervento).OrderBy(s => s.Descrizione).OrderBy(f => f.DataScadenzaPagamento);
        }



        private void btnSalvaProroga_Click(object sender, RoutedEventArgs e)
        {
            _ctx.SubmitChanges();
        }

       


        private void dgPagamenti_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Template = (ControlTemplate)FindResource("NewRow_ControlTemplate");
                e.Row.UpdateLayout();
                e.Row.MouseLeftButtonDown += Row_MouseLeftButtonDownPagamenti;
            }
        }

        private void dgPagamenti_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (((PagamentoIntervento)e.Row.Item).idPagamento == Guid.Empty)
                ((PagamentoIntervento)e.Row.Item).idPagamento = Guid.NewGuid();
            ((PagamentoIntervento)e.Row.Item).idIntervento = _intervento.idIntervento;

            CallDispatcher();
        }
        private void auto_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoIntervento)((CheckBox)sender).DataContext).Bonifico = vOut;
        }

        private void auto_Checked_1(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoIntervento)((CheckBox)sender).DataContext).Pagato = vOut;
        }

        private void auto_Checked_2(object sender, RoutedEventArgs e)
        {

            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoIntervento)((CheckBox)sender).DataContext).Insoluto = vOut;
        }


        private void DatePicker_SelectedDateChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ((PagamentoIntervento)((DatePicker)sender).DataContext).DataScadenzaPagamento = ((DatePicker)sender).SelectedDate;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var banca = (Banca)(((ComboBox)sender).SelectedItem);
            if (banca != null)
            {
                var idBanca = banca.idBanca;
                ((PagamentoIntervento)((ComboBox)sender).DataContext).idBanca = idBanca;
            }
        }

        void Row_MouseLeftButtonDownPagamenti(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row.Item == CollectionView.NewItemPlaceholder && row.Template == (ControlTemplate)FindResource("NewRow_ControlTemplate"))
            {
                row.ClearValue(DataGridRow.TemplateProperty);
                row.Focus();
                row.IsSelected = true;
                row.UpdateLayout();
                dgPagamenti.CurrentItem = row.Item;
                dgPagamenti.BeginEdit();
            }
        }
        private void CallDispatcher()
        {
            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
            {

                var cs = _ctx.GetChangeSet();

                foreach (var ch in cs.Updates)
                {
                    if (ch.GetType().Name == "PagamentoIntervento")
                    {
                        var pagamento = (PagamentoIntervento)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_intervento.Iva / 100)));
                    }
                }

                foreach (var ch in cs.Inserts)
                {
                    if (ch.GetType().Name == "PagamentoIntervento")
                    {
                        var pagamento = (PagamentoIntervento)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_intervento.Iva / 100)));
                    }
                }
                _ctx.SubmitChanges();

                decimal totalePagamenti = TotalePagamenti();
                lblTotale.Content = _intervento.ImportoTotaleIvato - totalePagamenti;


                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        private decimal TotalePagamenti()
        {
            decimal totalePagamenti = 0;
            foreach (var pag in _ctx.PagamentoInterventos.Where(s => s.idIntervento == _intervento.idIntervento))
            {
                totalePagamenti += (decimal)pag.Importo;
            }
            return totalePagamenti;
        }

        private void auto_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            if (isChecked)
            {
                var win = new DataPagamento();
                win.ShowDialog();
                if (win.DataPagamentoScelta != null)
                    ((PagamentoIntervento)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
                _ctx.SubmitChanges();
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ((PagamentoIntervento)((DatePicker)sender).DataContext).DataAcconto = ((DatePicker)sender).SelectedDate;
        }





    }
}
