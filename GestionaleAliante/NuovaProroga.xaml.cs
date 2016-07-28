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
    public partial class NuovaProroga : Window
    {


        Noleggio _noleggio = new Noleggio();
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        Proroga _proroga = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nol">Il Noleggio da prorogare</param>
        public NuovaProroga(Noleggio nol)
        {
            InitializeComponent();
            _noleggio = nol;
            dpProrogaDal.SelectedDate = CalcolaInizioProssimaProroga();
            dpProrogaAl.SelectedDate = CalcolaInizioProssimaProroga().AddMonths(1);
            txtImporto.Text = (_noleggio.CostoMetroQuadroProroga * (decimal)_noleggio.MetriQuadri).ToString();
            if (_noleggio.CostoMetroQuadroProroga == null)
            {
                MessageBox.Show("Inserisci il costo al Mq per le proroghe");
                this.Close();
            }

            if (_ctx.Prorogas.Where(s => s.idNoleggio == _noleggio.idNoleggio).Count() > 0)
            {
                txtNumeroProroga.Text = (_ctx.Prorogas.Where(s => s.idNoleggio == _noleggio.idNoleggio).Count() + 1).ToString();
            }
            else
                txtNumeroProroga.Text = "1";

            txtIva.Text = _noleggio.IVA.ToString();
            if (_proroga != null)
                if (_ctx.PagamentoProrogas.Where(s => s.idProroga == _proroga.IdProrogra).Count() > 0)
                    dgPagamenti.ItemsSource = _ctx.PagamentoProrogas.Where(s => s.idProroga == _proroga.IdProrogra).OrderBy(s => s.Descrizione).OrderBy(f => f.DataScadenzaPagamento);
        }
        /// <summary>
        /// Inizializza i campi
        /// </summary>
        /// <param name="idProroga"></param>
        /// <param name="nol"></param>
        public NuovaProroga(Guid idProroga, Noleggio nol)
        {
            InitializeComponent();
            _noleggio = nol;
            RicaricaProroga(idProroga);
        }

        private void RicaricaProroga(Guid idProroga)
        {
            var proroga = _ctx.Prorogas.Where(s => s.IdProrogra == idProroga).First();
            dpProrogaDal.SelectedDate = proroga.DataInizio;
            dpProrogaAl.SelectedDate = proroga.DataFine;
            dpProrogaScadenza.SelectedDate = proroga.DataScadenzaPagamento;
            ControllaProrogheMinoriDiTrentaGiorni();
            txtDescrizioneProroga.Text = proroga.Descrizione;
            txtNumeroProroga.Text = proroga.NumeroProroga.ToString();
            txtImporto.Text = proroga.Importo.ToString();

            if (proroga.Iva != null)
                txtIva.Text = proroga.Iva.ToString();
            else
                txtIva.Text = _noleggio.IVA.ToString();

            if (!String.IsNullOrEmpty(proroga.IntestazioneProroga))
                txtIntestazione.Text = proroga.IntestazioneProroga;

            if (proroga.IsCorpo == 1)
                cbxCorpo.IsChecked = true;
            else
                cbxCorpo.IsChecked = false;

            _proroga = proroga;

            decimal totalePagamenti = TotalePagamenti();
            lblTotale.Content = _proroga.ImportoTotaleIvato - totalePagamenti;
            dgPagamenti.ItemsSource = _ctx.PagamentoProrogas.Where(s => s.idProroga == _proroga.IdProrogra).OrderBy(s => s.Descrizione).OrderBy(f => f.DataScadenzaPagamento);

        }

        private DateTime CalcolaInizioProssimaProroga()
        {
            DateTime dataInizioProroga = DateTime.Now;
            if (_noleggio.dataProssimaProroga == null)
                dataInizioProroga = (DateTime)_noleggio.dataFineNoleggio;
            else
                dataInizioProroga = (DateTime)_noleggio.dataProssimaProroga;
            return dataInizioProroga;
        }

        private void btnSalvaProroga_Click(object sender, RoutedEventArgs e)
        {
            if (_proroga == null)
            {
                _proroga = new Proroga();
                _proroga.IdProrogra = Guid.NewGuid();
                _ctx.Prorogas.InsertOnSubmit(_proroga);
                _ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().Prorogas.Add(_proroga);
                Fattura fatt = new Fattura();
                fatt.IdFattura = Guid.NewGuid();
                fatt.FatturaProroga = 1;
                fatt.FatturaCantiere = 0;
                _proroga.Fattura = fatt;
            }
            _proroga.DataScadenzaPagamento = dpProrogaScadenza.SelectedDate;
            _proroga.DataInizio = dpProrogaDal.SelectedDate;
            _proroga.DataFine = dpProrogaAl.SelectedDate;
            _proroga.Importo = Convert.ToDecimal(txtImporto.Text);
            _proroga.Iva = Convert.ToInt32(txtIva.Text);
            _proroga.ImportoTotaleIvato = _proroga.Importo + (_proroga.Importo / 100) * (decimal)_proroga.Iva;
            _proroga.ImportoIva = (_proroga.Importo / 100) * (decimal)_proroga.Iva;
            decimal totalePagamenti = TotalePagamenti();
            lblTotale.Content = _proroga.ImportoTotaleIvato - totalePagamenti;
            _proroga.Descrizione = txtDescrizioneProroga.Text;
            _proroga.IntestazioneProroga = txtIntestazione.Text;
            if ((bool)cbxCorpo.IsChecked)
                _proroga.IsCorpo = 1;
            else
                _proroga.IsCorpo = 0;
            _proroga.NumeroProroga = Convert.ToInt32(txtNumeroProroga.Text);

            if (_ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().dataProssimaProroga < _proroga.DataFine || _ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().dataProssimaProroga == null)
                _ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().dataProssimaProroga = _proroga.DataFine;

            _ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().NumeroProroga = _proroga.NumeroProroga;

            _ctx.SubmitChanges();

            RicaricaProroga(_proroga.IdProrogra);

        }

        private void dpProrogaAl_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ControllaProrogheMinoriDiTrentaGiorni();
        }

        private void ControllaProrogheMinoriDiTrentaGiorni()
        {
            if (dpProrogaAl.SelectedDate != null && dpProrogaDal.SelectedDate != null)
                if (dpProrogaAl.SelectedDate.GetValueOrDefault().AddMonths(-1) < dpProrogaDal.SelectedDate)
                {
                    var giorni = dpProrogaAl.SelectedDate - dpProrogaDal.SelectedDate;
                    if (!(bool)cbxCorpo.IsChecked)
                        txtImporto.Text = (((_noleggio.CostoMetroQuadroProroga * (decimal)_noleggio.MetriQuadri) / 30) * giorni.GetValueOrDefault().Days).ToString();
                }
                else
                    if (!(bool)cbxCorpo.IsChecked)
                        txtImporto.Text = (_noleggio.CostoMetroQuadroProroga * (decimal)_noleggio.MetriQuadri).ToString();
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
            if (((PagamentoProroga)e.Row.Item).idPagamento == Guid.Empty)
                ((PagamentoProroga)e.Row.Item).idPagamento = Guid.NewGuid();
            ((PagamentoProroga)e.Row.Item).idProroga = _proroga.IdProrogra;

            CallDispatcher();
        }
        private void auto_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoProroga)((CheckBox)sender).DataContext).Bonifico = vOut;
        }

        private void auto_Checked_1(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoProroga)((CheckBox)sender).DataContext).Pagato = vOut;
        }

        private void auto_Checked_2(object sender, RoutedEventArgs e)
        {

            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((PagamentoProroga)((CheckBox)sender).DataContext).Insoluto = vOut;
        }


        private void DatePicker_SelectedDateChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ((PagamentoProroga)((DatePicker)sender).DataContext).DataScadenzaPagamento = ((DatePicker)sender).SelectedDate;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var banca = (Banca)(((ComboBox)sender).SelectedItem);
            if (banca != null)
            {
                var idBanca = banca.idBanca;
                ((PagamentoProroga)((ComboBox)sender).DataContext).idBanca = idBanca;
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
                    if (ch.GetType().Name == "PagamentoProroga")
                    {
                        var pagamento = (PagamentoProroga)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_noleggio.IVA / 100)));
                    }
                }

                foreach (var ch in cs.Inserts)
                {
                    if (ch.GetType().Name == "PagamentoProroga")
                    {
                        var pagamento = (PagamentoProroga)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_noleggio.IVA / 100)));
                    }
                }
                _ctx.SubmitChanges();

                decimal totalePagamenti = TotalePagamenti();
                lblTotale.Content = _proroga.ImportoTotaleIvato - totalePagamenti;
                //RicaricaDati();
                //Calcola ImportoIvato Interventi

                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        private decimal TotalePagamenti()
        {
            decimal totalePagamenti = 0;
            foreach (var pag in _ctx.PagamentoProrogas.Where(s => s.idProroga == _proroga.IdProrogra))
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
                    ((PagamentoProroga)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
                _ctx.SubmitChanges();
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ((PagamentoProroga)((DatePicker)sender).DataContext).DataAcconto = ((DatePicker)sender).SelectedDate;
        }





    }
}
