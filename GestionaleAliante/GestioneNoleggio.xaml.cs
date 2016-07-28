using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for GestioneNoleggio.xaml
    /// </summary>
    public partial class GestioneNoleggio : Window
    {




        #region Variabili Globali

        Noleggio _noleggio = new Noleggio();
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();

        #endregion

        #region Inizializza

        public GestioneNoleggio()
        {
            InitializeComponent();
            cbxCorpo.IsChecked = true;
            cbxCorpo.IsChecked = false;
            CaricaCombos();
        }

        public GestioneNoleggio(Guid guid)
        {
            InitializeComponent();
            RicaricaNoleggio(guid);
        }

        private void RicaricaNoleggio(Guid guid)
        {
            _ctx = new AlianteLinqDataContext();
            cbxCorpo.IsChecked = true;
            cbxCorpo.IsChecked = false;

            _noleggio = _ctx.Noleggios.Where(s => s.idNoleggio == guid).First();
            txtCostoMetroQuadro.Text = _noleggio.CostoMetroQuadro.ToString();
            txtCostoMetroQuadroProroga.Text = _noleggio.CostoMetroQuadroProroga.ToString();
            txtIVA.Text = _noleggio.IVA.ToString();
            txtCostoCorpo.Text = _noleggio.CostoCorpo.ToString();
            txtCostoCorpoProroga.Text = _noleggio.CostoCorpoProroga.ToString();

            txtMetriQuadri.Text = _noleggio.MetriQuadri.ToString();
            dateFine.Text = _noleggio.dataFineNoleggio.ToString();
            dateInizio.Text = _noleggio.dataComincioNoleggio.ToString();
            if (_noleggio.NoleggioChiuso == 1)
            {
                cbxNoleggioChiuso.IsChecked = true;
                DisabilitaPannelli();
            }

            if (_noleggio.IsCorpo == 1)
                cbxCorpo.IsChecked = true;

            txtNote.Text = _noleggio.note;
            txtDescrizionePagamento.Text = _noleggio.DescrizioneNoleggio;
            if (_noleggio.Fattura != null)
            {
                dpDataPrimaFattura.SelectedDate = _noleggio.Fattura.DataFattura;
                txtNumeroPrimaFattura.Text = _noleggio.Fattura.NumeroFattura.ToString();
            }
            else
            {
                dpDataPrimaFattura.SelectedDate = DateTime.Today;
            }
            CaricaCombos();

            //ALTRI TAB
            RicaricaDati();
            //CaricaFattureProroghe();
        }

        #endregion

        public void CaricaCombos()
        {
            cmbBanca.ItemsSource = _ctx.Bancas.OrderBy(s => s.NomeBanca);
            cmbBanca.DisplayMemberPath = "NomeBanca";

            if (_noleggio != null)
                if (_noleggio.Banca != null)
                    cmbBanca.SelectedItem = _noleggio.Banca;

            CaricaComboClienti();

            if (_noleggio != null)
            {
                if (_noleggio.Indirizzo == null)
                {
                    _noleggio.Indirizzo = new Indirizzo();
                    _noleggio.Indirizzo.IdIndirizzo = Guid.NewGuid();
                }
                gbIndirizzoCantiere.DataContext = _noleggio.Indirizzo;
            }



        }


        private bool Valida()
        {

            var result = true;
            if (cbxCorpo != null)
            {
                if (!(bool)cbxCorpo.IsChecked)
                {
                    if (string.IsNullOrEmpty(txtCostoMetroQuadro.Text))
                    {
                        result = false;
                        txtCostoMetroQuadro.BorderBrush = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        txtCostoMetroQuadro.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(txtCostoCorpo.Text))
                    {
                        result = false;
                        txtCostoCorpo.BorderBrush = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        txtCostoCorpo.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }

                    if (string.IsNullOrEmpty(txtCostoCorpoProroga.Text))
                    {
                        result = false;
                        txtCostoCorpoProroga.BorderBrush = System.Windows.Media.Brushes.Red;
                    }
                    else
                    {
                        txtCostoCorpoProroga.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }
                }

                if (string.IsNullOrEmpty(txtIVA.Text))
                {
                    result = false;
                    txtIVA.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    txtIVA.BorderBrush = System.Windows.Media.Brushes.Transparent;
                }

            }
            return result;
        }

        private void RicaricaDati()
        {
            dgNoteDiCredito.ItemsSource = _ctx.NotaCreditos.Where(s => s.idNoleggio == _noleggio.idNoleggio);
            dgAltriCosti.ItemsSource = _ctx.AltriCostis.Where(s => s.idNoleggio == _noleggio.idNoleggio);
            dgPagamenti.ItemsSource = _ctx.Pagamentos.Where(s => s.idNoleggio == _noleggio.idNoleggio).OrderBy(s => s.Descrizione).OrderBy(f => f.DataScadenzaPagamento);
            dgInterventi.ItemsSource = _ctx.Interventos.Where(s => s.idNoleggio == _noleggio.idNoleggio).OrderBy(s => s.Descrizione).OrderBy(f => f.DataScadenzaPagamento);
            var result = from t in _ctx.Materiales
                         join x in _ctx.MaterialeNoleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio) on t.idMateriale equals x.idMateriale
                         into JoinedEmpDept
                         from dept in JoinedEmpDept.DefaultIfEmpty()
                         select new
                         {
                             idMateriale = t.idMateriale,
                             DescrizioneMateriale = t.DescrizioneMateriale,
                             CodiceMateriale = t.CodiceMateriale,
                             Ordine = t.Ordine,
                             idNoleggio = dept != null ? dept.idNoleggio : Guid.NewGuid(),
                             Quantità = dept != null ? dept.Quantità : 0
                         };

            dgMateriali.ItemsSource = result.OrderBy(s => s.Ordine);

            lbCamion.ItemsSource = _ctx.Camions;
            RicaricaDettaglioPagamenti();
            InizializzaProrogheEInterventi();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Salva();
        }

        private void Salva()
        {
            if (_noleggio.idNoleggio == Guid.Empty)
            {
                _noleggio.idNoleggio = Guid.NewGuid();
                _ctx.Noleggios.InsertOnSubmit(_noleggio);
            }

            if (!String.IsNullOrEmpty(txtCostoMetroQuadro.Text))
                _noleggio.CostoMetroQuadro = Convert.ToDecimal(txtCostoMetroQuadro.Text);
            else
                _noleggio.CostoMetroQuadro = 0;

            if (!String.IsNullOrEmpty(txtCostoMetroQuadroProroga.Text))
                _noleggio.CostoMetroQuadroProroga = Convert.ToDecimal(txtCostoMetroQuadroProroga.Text);
            else
                _noleggio.CostoMetroQuadroProroga = 0;

            if (!String.IsNullOrEmpty(txtCostoCorpo.Text))
                _noleggio.CostoCorpo = Convert.ToDecimal(txtCostoCorpo.Text);
            else
                _noleggio.CostoCorpo = 0;
            if (!String.IsNullOrEmpty(txtCostoCorpoProroga.Text))
                _noleggio.CostoCorpoProroga = Convert.ToDecimal(txtCostoCorpoProroga.Text);
            else
                _noleggio.CostoCorpoProroga = 0;

            if (!String.IsNullOrEmpty(txtIVA.Text))
                _noleggio.IVA = Convert.ToDouble(txtIVA.Text);
            if (!String.IsNullOrEmpty(txtMetriQuadri.Text))
                _noleggio.MetriQuadri = Convert.ToDouble(txtMetriQuadri.Text);
            else
                _noleggio.MetriQuadri = 0;

            if (!String.IsNullOrEmpty(dateFine.Text))
                _noleggio.dataFineNoleggio = Convert.ToDateTime(dateFine.Text);
            if (!String.IsNullOrEmpty(dateInizio.Text))
                _noleggio.dataComincioNoleggio = Convert.ToDateTime(dateInizio.Text);

            if (txtNote.Text != null)
                _noleggio.note = txtNote.Text;
            if (txtDescrizionePagamento.Text != null)
                _noleggio.DescrizioneNoleggio = txtDescrizionePagamento.Text;

            if (cmbBanca.SelectedItem != null)
                _noleggio.Banca = (Banca)cmbBanca.SelectedItem;

            if (cmbCliente.SelectedItem != null)
                _noleggio.Cliente = (Cliente)cmbCliente.SelectedItem;

            if ((bool)cbxNoleggioChiuso.IsChecked)
                _noleggio.NoleggioChiuso = 1;
            else
                _noleggio.NoleggioChiuso = 0;

            if ((bool)cbxCorpo.IsChecked)
                _noleggio.IsCorpo = 1;
            else
                _noleggio.IsCorpo = 0;

            if (_noleggio.dataProssimaProroga == null)
                _noleggio.dataProssimaProroga = dateFine.SelectedDate;

            _ctx.SubmitChanges();

            RicaricaNoleggio(_noleggio.idNoleggio);
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {

            MessageBoxResult result = MessageBox.Show("Vuoi salvare le modifiche?", "Confirmation", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                Salva();
            }
            else if (result == MessageBoxResult.No)
            {

            }
            else
            {
                e.Cancel = true;
            }

        }

        private void CallDispatcher()
        {
            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
            {

                var cs = _ctx.GetChangeSet();

                foreach (var ch in cs.Updates)
                {
                    if (ch.GetType().Name == "Pagamento")
                    {
                        var pagamento = (Pagamento)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_noleggio.IVA / 100)));
                    }
                }

                foreach (var ch in cs.Inserts)
                {
                    if (ch.GetType().Name == "Pagamento")
                    {
                        var pagamento = (Pagamento)ch;
                        pagamento.ImportoIva = pagamento.Importo - (pagamento.Importo / (decimal)(1 + (_noleggio.IVA / 100)));
                    }
                }
                _ctx.SubmitChanges();
                RicaricaDati();
                //Calcola ImportoIvato Interventi

                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        private void txtCostoMetroQuadro_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Valida())
            {
                if (btnSalva != null)
                    btnSalva.IsEnabled = true;
            }
            else
                if (btnSalva != null)
                btnSalva.IsEnabled = false;
        }

        private void dateInizio_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Valida())
                btnSalva.IsEnabled = true;
            else
                btnSalva.IsEnabled = false;
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ClienteGestione newW = new ClienteGestione();
            newW.ShowDialog();

            CaricaComboClienti();
        }

        private void CaricaComboClienti()
        {
            if (cmbCliente != null)
            {
                MultiBindingExpression binding = BindingOperations.GetMultiBindingExpression(cmbCliente, ComboBox.ItemsSourceProperty);
                if (binding != null)
                {
                    binding.UpdateTarget();
                }
            }
            cmbCliente.ItemsSource = _ctx.Clientes.OrderBy(s => s.RagioneSociale);
            cmbCliente.DisplayMemberPath = "RagioneSociale";
            if (_noleggio != null)
                if (_noleggio.Cliente != null)
                    cmbCliente.SelectedItem = _noleggio.Cliente;
        }

        void Row_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row.Item == CollectionView.NewItemPlaceholder && row.Template == (ControlTemplate)FindResource("NewRow_ControlTemplate"))
            {
                row.ClearValue(DataGridRow.TemplateProperty);
                row.Focus();
                row.IsSelected = true;
                row.UpdateLayout();
                dgAltriCosti.CurrentItem = row.Item;
                dgAltriCosti.BeginEdit();
            }
        }

        private void DatePicker_SelectedDateChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ((Pagamento)((DatePicker)sender).DataContext).DataScadenzaPagamento = ((DatePicker)sender).SelectedDate;
        }


        #region tabAltriCosti

        private void CallDispatcherAltriCosti()
        {
            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
            {
                _ctx.SubmitChanges();
                RicaricaDati();
                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        #region datagrid

        private void dgAltriCosti_LoadingRow(object sender, DataGridRowEventArgs e)
        {

            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Template = (ControlTemplate)FindResource("NewRow_ControlTemplate");
                e.Row.UpdateLayout();
                e.Row.MouseLeftButtonDown += Row_MouseLeftButtonDown;
            }
        }

        #endregion
        private void dgAltriCosti_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

            if (((AltriCosti)e.Row.Item).idAltriCosti == Guid.Empty)
                ((AltriCosti)e.Row.Item).idAltriCosti = Guid.NewGuid();
            ((AltriCosti)e.Row.Item).idNoleggio = _noleggio.idNoleggio;

            CallDispatcherAltriCosti();

        }

        private void dgAltriCosti_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cs = _ctx.GetChangeSet();
            foreach (AltriCosti item in cs.Inserts)
            {
                if (item.Descrizione == null && item.Importo == null)
                    _ctx.AltriCostis.DeleteOnSubmit(item);
            }
            _ctx.SubmitChanges();
            RicaricaDettaglioPagamenti();
        }
        #endregion

        #region tabPagamenti
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

        private void dgPagamenti_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Template = (ControlTemplate)FindResource("NewRow_ControlTemplate");
                e.Row.UpdateLayout();
                e.Row.MouseLeftButtonDown += Row_MouseLeftButtonDownPagamenti;
            }
            //else
            //{
            //    CheckBox chkImport = Fin FindVisualChild<CheckBox>(e.Row);
            //    var pag = (Pagamento)e.Row.Item;
            //    var cmb = e.Row.FindName("cmbBanchePag");
            //}
        }

        private void dgPagamenti_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (((Pagamento)e.Row.Item).idPagamento == Guid.Empty)
                ((Pagamento)e.Row.Item).idPagamento = Guid.NewGuid();
            ((Pagamento)e.Row.Item).idNoleggio = _noleggio.idNoleggio;

            CallDispatcher();
        }


        private void RicaricaDettaglioPagamenti()
        {
            var importoPagamenti = _ctx.Pagamentos.Where(s => s.idNoleggio == _noleggio.idNoleggio).Sum(s => s.Importo);
            if (importoPagamenti != null)
                lblTotalePagamentiInseriti.Content = ((decimal)importoPagamenti).ToString("c");


            if (_noleggio.IsCorpo != 1)
            {
                if (_noleggio.MetriQuadri != null && _noleggio.IVA != null && _noleggio.CostoMetroQuadro != null)
                {
                    var importoTotaleAltriCosti = _ctx.AltriCostis.Where(s => s.idNoleggio == _noleggio.idNoleggio).Sum(s => s.Importo);
                    if (importoTotaleAltriCosti == null)
                        importoTotaleAltriCosti = 0;
                    var importoTotale = ((decimal)_noleggio.MetriQuadri * _noleggio.CostoMetroQuadro) + importoTotaleAltriCosti;
                    var iva = (importoTotale / 100) * (decimal)_noleggio.IVA;
                    lblTotale.Content = ((decimal)importoTotale).ToString("c");
                    lblIVA.Content = ((decimal)iva).ToString("c");
                    lblSomma.Content = ((decimal)(importoTotale + iva)).ToString("c");

                    if ((importoTotale + iva) == importoPagamenti)
                    {
                        lblTotalePagamentiInseriti.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        lblTotalePagamentiInseriti.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
            else
            {
                var importoTotaleAltriCosti = _ctx.AltriCostis.Where(s => s.idNoleggio == _noleggio.idNoleggio).Sum(s => s.Importo);
                if (importoTotaleAltriCosti == null)
                    importoTotaleAltriCosti = 0;
                var importoTotale = _noleggio.CostoCorpo + importoTotaleAltriCosti;
                var iva = (importoTotale / 100) * (decimal)_noleggio.IVA;
                lblTotale.Content = ((decimal)importoTotale).ToString("c");
                lblIVA.Content = ((decimal)iva).ToString("c");
                lblSomma.Content = ((decimal)(importoTotale + iva)).ToString("c");

                if ((importoTotale + iva) == importoPagamenti)
                {
                    lblTotalePagamentiInseriti.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    lblTotalePagamentiInseriti.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

        private void auto_Checked_1(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Pagamento)((CheckBox)sender).DataContext).Pagato = vOut;
            //if (isChecked)
            //{
            //    var win = new DataPagamento();
            //    win.ShowDialog();
            //    if (win.DataPagamentoScelta != null)
            //        ((Pagamento)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
            //    _ctx.SubmitChanges();
            //}

        }

        private void auto_Checked_2(object sender, RoutedEventArgs e)
        {

            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Pagamento)((CheckBox)sender).DataContext).Insoluto = vOut;
        }
        #endregion

        #region tabDocumenti


        #region NotaSpese
        private void imgPdfNotaSpese_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_noleggio.PdfNotaSpese == null || cbRicrea.IsChecked == true)
                _noleggio.PdfNotaSpese = GestioneReport.PathReport(_noleggio.idNoleggio.ToString(), _noleggio.idBanca.ToString(), _noleggio.idCliente.ToString(), _noleggio.idIndirizzo.ToString());

            GestioneReport.MostraPdf(_noleggio.PdfNotaSpese.ToArray());
            _ctx.SubmitChanges();

        }
        #endregion


        #region PrimaFattura
        private void imgPdfPrimaFattura_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtDescrizionePagamento.Text != "")
            {
                if ((bool)cbxBonificoPrimaFattura.IsChecked)
                    CreaOVisualizzaFattura();
                else
                    if (_noleggio.idFattura == null)
                {
                    if (ControllaBanca())
                        CreaOVisualizzaFattura();
                }
                else
                    CreaOVisualizzaFattura();
            }
            else
            {
                MessageBox.Show("Inserisci la descrizione del pagamento");
            }



        }

        private void CreaOVisualizzaFattura()
        {
            if (_noleggio.idFattura == null || cbRicreaFattura.IsChecked == true)
            {
                if (_noleggio.Fattura != null)
                {
                    _ctx.Fatturas.DeleteOnSubmit(_noleggio.Fattura);
                    _noleggio.Fattura = null;
                }
                _ctx.SubmitChanges();
                var fattura = new Fattura();
                fattura.FatturaProroga = 0;
                fattura.FatturaCantiere = 1;
                _ctx.Fatturas.InsertOnSubmit(fattura);
                _noleggio.Fattura = fattura;
                _noleggio.Fattura.IdFattura = Guid.NewGuid();
                _noleggio.Fattura.DataFattura = dpDataPrimaFattura.SelectedDate;
                _noleggio.Fattura.NumeroFattura = Convert.ToInt32(txtNumeroPrimaFattura.Text);
                _ctx.SubmitChanges();
                _noleggio.Fattura.PdfFattura = GestioneReport.GeneraFattura(_noleggio, (bool)cbxBonificoPrimaFattura.IsChecked);
                _ctx.SubmitChanges();
            }

            GestioneReport.MostraPdf(_noleggio.Fattura.PdfFattura.ToArray());
        }
        #endregion


        private bool ControllaBanca()
        {
            if (_noleggio.Cliente.Banca != null)
                if (string.IsNullOrEmpty(_noleggio.Cliente.Banca.ABI) || string.IsNullOrEmpty(_noleggio.Cliente.Banca.CAB))
                {
                    MessageBox.Show("Associa il cliente ad una banca d'appoggio \rSono necessari ABI e CAB");
                    return false;
                }
                else
                    return true;
            MessageBox.Show("Associa il cliente ad una banca d'appoggio \rSono necessari ABI e CAB");
            return false;
        }

        #region Contratto
        private void Image_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            CaricaContratto();
        }

        private void CaricaContratto()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "pdf Files (*.pdf)|*.pdf|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                if (!string.IsNullOrEmpty(filename))
                {
                    _noleggio.PdfContratto = new Binary(File.ReadAllBytes(filename));
                    _ctx.SubmitChanges();
                }
                MessageBox.Show("Contratto Caricato");
            }
        }

        private void imgPdf_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_noleggio.PdfContratto == null || cbRicaricaContratto.IsChecked == true)
            {
                CaricaContratto();
            }
            VisualizzaContratto();
        }

        private void VisualizzaContratto()
        {
            if (_noleggio.PdfContratto != null)
                GestioneReport.MostraPdf(_noleggio.PdfContratto.ToArray());
        }

        #endregion

        #region FattureProroga

        List<Fattura> fattureDaRicaricare = new List<Fattura>();
        private void imgPdfFatturaProroga_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var id = ((Fattura)((Image)sender).DataContext).IdFattura;
            Fattura fatt = new Fattura();

            foreach (var item in dgFattureProroghe.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    fatt = (Fattura)item;
            }


            if (fattureConBonifico.Contains(fatt))
                CreaOVisualizzaFatturaProroga(sender);
            else
                if (fatt.PdfFattura == null)
            {
                if (ControllaBanca())
                    CreaOVisualizzaFatturaProroga(sender);
            }
            else
                CreaOVisualizzaFatturaProroga(sender);
        }

        private void CreaOVisualizzaFatturaProroga(object sender)
        {
            var id = ((Fattura)((Image)sender).DataContext).IdFattura;
            Fattura fatt = new Fattura();

            foreach (var item in dgFattureProroghe.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    fatt = (Fattura)item;
            }

            _ctx.SubmitChanges();


            if (fatt.PdfFattura == null || fattureDaRicaricare.Contains(fatt))
            {
                bool bonifico = false;

                if (fattureConBonifico.Contains(fatt))
                    bonifico = true;
                fatt.PdfFattura = GestioneReport.GeneraFatturaProroga(fatt, bonifico); ;
                _ctx.SubmitChanges();
            }

            GestioneReport.MostraPdf(fatt.PdfFattura.ToArray());
        }

        private void DatePicker_SelectedDateChanged_2(object sender, SelectionChangedEventArgs e)
        {
            var id = ((Fattura)((DatePicker)sender).DataContext).IdFattura;
            foreach (var item in dgFattureProroghe.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    ((Fattura)item).DataFattura = ((DatePicker)sender).SelectedDate;
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var id = ((Fattura)((TextBox)sender).DataContext).IdFattura;
            foreach (var item in dgFattureProroghe.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    if (!string.IsNullOrEmpty(((TextBox)sender).Text))
                        ((Fattura)item).NumeroFattura = Convert.ToInt32(((TextBox)sender).Text);
            }
        }

        private void cbxRicreaFatturaProroga_Checked(object sender, RoutedEventArgs e)
        {
            var cbx = (CheckBox)sender;
            var fatt = (Fattura)(cbx.DataContext);
            if ((bool)cbx.IsChecked)
                fattureDaRicaricare.Add(fatt);
            else
                fattureDaRicaricare.Remove(fatt);

        }

        List<Fattura> fattureConBonifico = new List<Fattura>();
        private void cbxBonifico_Checked(object sender, RoutedEventArgs e)
        {
            var cbx = (CheckBox)sender;
            var fatt = (Fattura)(cbx.DataContext);
            if ((bool)cbx.IsChecked)
                fattureConBonifico.Add(fatt);
            else
                fattureConBonifico.Remove(fatt);
        }
        #endregion

        #endregion

        #region tabProroghe

        private void InizializzaProrogheEInterventi()
        {

            lblProssimaProroga.Content = CalcolaInizioProssimaProroga().ToString("dd/MM/yyyy");
            dgProroghe.ItemsSource = _noleggio.Prorogas.OrderByDescending(s => s.DataFine);


            //dgNoteDiCredixto.ItemsSource = _noleggio.No;
            CaricaFattureProrogheEInterventi();

        }

        private void imgAggiungiProroga_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var win = new NuovaProroga(_noleggio);
            try
            {
                win.ShowDialog();
            }
            catch (Exception)
            {


            }
            var id = _noleggio.idNoleggio;
            _ctx = new AlianteLinqDataContext();
            _noleggio = _ctx.Noleggios.Where(s => s.idNoleggio == id).First();
            RicaricaNoleggio(id);
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

        private void auto_Checked_3(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Proroga)((CheckBox)sender).DataContext).Pagato = vOut;
            _ctx.SubmitChanges();
        }

        private void auto_Checked_4(object sender, RoutedEventArgs e)
        {

            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Proroga)((CheckBox)sender).DataContext).Insoluto = vOut;
            _ctx.SubmitChanges();
        }



        private void AButton_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Vuoi Eliminare questa proroga?", "Confirmation", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var btn = (Button)sender;
                var id = btn.CommandParameter.ToString();
                var guid = new Guid(id);
                UtilityNoleggio.EliminaProroga(guid);
                _noleggio.NumeroProroga = _noleggio.NumeroProroga - 1;
                _ctx.SubmitChanges();
                RicaricaNoleggio(_noleggio.idNoleggio);
            }
        }

        private void AButton_Click_1(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var id = btn.CommandParameter.ToString();
            var guid = new Guid(id);
            var newW = new NuovaProroga(guid, _noleggio);
            newW.ShowDialog();
            RicaricaNoleggio(_noleggio.idNoleggio);
        }




        #endregion

        private void CaricaFattureProrogheEInterventi()
        {
            var proroghe = _noleggio.Prorogas;
            List<Fattura> fattureProroghe = new List<Fattura>();
            foreach (var proroga in proroghe)
            {
                if (proroga.Fattura != null)
                    fattureProroghe.Add(proroga.Fattura);
            }
            dgFattureProroghe.ItemsSource = fattureProroghe.OrderByDescending(s => s.DataFattura);


            var interventi = _noleggio.Interventos;
            List<Fattura> fattureInterventi = new List<Fattura>();
            foreach (var intervento in interventi)
            {
                if (intervento.Fattura != null)
                    fattureInterventi.Add(intervento.Fattura);
            }
            dgFattureInterventi.ItemsSource = fattureInterventi.OrderByDescending(s => s.DataFattura);

            var noteDiCredito = _noleggio.NotaCreditos;
            List<Fattura> fattureNoteDiCredito = new List<Fattura>();
            foreach (var notaDiCredito in noteDiCredito)
            {
                if (notaDiCredito.Fattura != null)
                    fattureNoteDiCredito.Add(notaDiCredito.Fattura);
            }
            dgFattureNotaDiCredito.ItemsSource = fattureNoteDiCredito.OrderByDescending(s => s.DataFattura);

        }

        private void txtDescrizionePagamento_TextChanged(object sender, TextChangedEventArgs e)
        {
            _noleggio.DescrizioneNoleggio = txtDescrizionePagamento.Text;
            _ctx.SubmitChanges();
        }

        private void cbxNoleggioChiuso_Checked(object sender, RoutedEventArgs e)
        {
            DisabilitaPannelli();
        }

        private void DisabilitaPannelli()
        {
            grAltriCosti.IsEnabled = false;
            grPagamenti.IsEnabled = false;
            grProroghe.IsEnabled = false;

            grNoleggio.IsEnabled = false;
            grInterventi.IsEnabled = false;


        }

        private void cbxNoleggioChiuso_Unchecked(object sender, RoutedEventArgs e)
        {
            grAltriCosti.IsEnabled = true;
            grPagamenti.IsEnabled = true;
            grProroghe.IsEnabled = true;
            grFatture.IsEnabled = true;
            grNoleggio.IsEnabled = true;
            grInterventi.IsEnabled = true;
            btnSalva.IsEnabled = true;
        }

        #region tabInterventi

        private void dgInterventi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_ctx.SubmitChanges();
        }

        private void dgInterventi_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (((Intervento)e.Row.Item).idIntervento == Guid.Empty)
                ((Intervento)e.Row.Item).idIntervento = Guid.NewGuid();
            if (((Intervento)e.Row.Item).Fattura == null)
            {
                var fatt = new Fattura();
                fatt.IdFattura = Guid.NewGuid();
                ((Intervento)e.Row.Item).Fattura = fatt;
            }
            ((Intervento)e.Row.Item).idNoleggio = _noleggio.idNoleggio;

            CallDispatcherInterventi();
        }

        private void CallDispatcherInterventi()
        {
            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
            {
                var cs = _ctx.GetChangeSet();

                foreach (var ch in cs.Updates)
                {
                    if (ch.GetType().Name == "Intervento")
                    {
                        var intervento = (Intervento)ch;
                        if (intervento.Iva == null)
                            intervento.Iva = _noleggio.IVA;
                        if (intervento.IsCorpo == 1)
                        {
                            intervento.ImportoIva = (intervento.Importo / 100) * (decimal)intervento.Iva;
                            intervento.ImportoTotaleIvato = intervento.Importo + (intervento.Importo / 100) * (decimal)intervento.Iva;
                        }
                        else
                        {
                            var totale = intervento.Importo * (decimal)intervento.Ore * intervento.NumeroAddetti;
                            intervento.ImportoIva = (totale / 100) * (decimal)intervento.Iva;
                            intervento.ImportoTotaleIvato = totale + (totale / 100) * (decimal)intervento.Iva;
                        }
                    }
                }

                foreach (var ch in cs.Inserts)
                {
                    if (ch.GetType().Name == "Intervento")
                    {
                        var intervento = (Intervento)ch;
                        if (intervento.Iva == null)
                            intervento.Iva = _noleggio.IVA;
                        if (intervento.IsCorpo == 1)
                        {
                            intervento.ImportoIva = (intervento.Importo / 100) * (decimal)intervento.Iva;
                            intervento.ImportoTotaleIvato = intervento.Importo + (intervento.Importo / 100) * (decimal)intervento.Iva;
                        }
                        else
                        {
                            var totale = intervento.Importo * (decimal)intervento.Ore * intervento.NumeroAddetti;
                            intervento.ImportoIva = (totale / 100) * (decimal)intervento.Iva;
                            intervento.ImportoTotaleIvato = totale + (totale / 100) * (decimal)intervento.Iva;
                        }
                    }
                }


                _ctx.SubmitChanges();
                //Calcola ImportoIvato Interventi
                RicaricaDati();


                return null;
            }), DispatcherPriority.Background, new object[] { null });
        }

        private void dgInterventi_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item == CollectionView.NewItemPlaceholder)
            {
                e.Row.Template = (ControlTemplate)FindResource("NewRow_ControlTemplate");
                e.Row.UpdateLayout();
                e.Row.MouseLeftButtonDown += Row_MouseLeftButtonDownPagamenti;
            }
        }

        private void auto_Checked_InterventiPagato(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Intervento)((CheckBox)sender).DataContext).Pagato = vOut;
        }

        private void auto_Checked_InterventiInsoluto(object sender, RoutedEventArgs e)
        {

            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Intervento)((CheckBox)sender).DataContext).Insoluto = vOut;
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            var id = ((Fattura)((TextBox)sender).DataContext).IdFattura;
            foreach (var item in dgFattureInterventi.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    if (!string.IsNullOrEmpty(((TextBox)sender).Text))
                        ((Fattura)item).NumeroFattura = Convert.ToInt32(((TextBox)sender).Text);
            }
        }

        private void DatePicker_SelectedDateChanged_4(object sender, SelectionChangedEventArgs e)
        {
            ((Intervento)((DatePicker)sender).DataContext).DataScadenzaPagamento = ((DatePicker)sender).SelectedDate;
        }

        private void imgPdfFatturaIntervento_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            var id = ((Fattura)((Image)sender).DataContext).IdFattura;
            Fattura fatt = new Fattura();

            foreach (var item in dgFattureInterventi.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    fatt = (Fattura)item;
            }


            if (fattureConBonifico.Contains(fatt))
                CreaOVisualizzaFatturaIntervento(sender);
            else
                if (fatt.PdfFattura == null)
            {
                if (ControllaBanca())
                    CreaOVisualizzaFatturaIntervento(sender);
            }
            else
                CreaOVisualizzaFatturaIntervento(sender);
        }

        private void CreaOVisualizzaFatturaIntervento(object sender)
        {
            var id = ((Fattura)((Image)sender).DataContext).IdFattura;
            Fattura fatt = new Fattura();

            foreach (var item in dgFattureInterventi.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    fatt = (Fattura)item;
            }

            _ctx.SubmitChanges();

            if (fatt.PdfFattura == null || fattureDaRicaricare.Contains(fatt))
            {
                bool bonifico = false;

                if (fattureConBonifico.Contains(fatt))
                    bonifico = true;
                fatt.PdfFattura = GestioneReport.GeneraFatturaIntervento(fatt, bonifico); ;
                _ctx.SubmitChanges();
            }

            GestioneReport.MostraPdf(fatt.PdfFattura.ToArray());
        }

        private void cbxIsCorpoIntervento_Checked_1(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Intervento)((CheckBox)sender).DataContext).IsCorpo = vOut;
        }

        private void DatePicker_SelectedDateChanged_3(object sender, SelectionChangedEventArgs e)
        {
            ((Intervento)((DatePicker)sender).DataContext).DataIntervento = ((DatePicker)sender).SelectedDate;
        }

        private void DatePicker_SelectedDateChanged_5(object sender, SelectionChangedEventArgs e)
        {
            var id = ((Fattura)((DatePicker)sender).DataContext).IdFattura;
            foreach (var item in dgFattureInterventi.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    ((Fattura)item).DataFattura = ((DatePicker)sender).SelectedDate;
            }
        }
        #endregion

        private void cbxCorpo_Unchecked(object sender, RoutedEventArgs e)
        {
            grNoleggioNormale.Visibility = Visibility.Visible;
            grNoleggioCorpo.Visibility = Visibility.Hidden;
        }

        private void cbxCorpo_Checked(object sender, RoutedEventArgs e)
        {
            grNoleggioNormale.Visibility = Visibility.Hidden;
            grNoleggioCorpo.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (_noleggio.Pagamentos.Count() > 0)
            {
                foreach (var nol in _noleggio.Pagamentos)
                {
                    _ctx.Pagamentos.DeleteOnSubmit(nol);
                }
                _ctx.SubmitChanges();
            }
            decimal importo = 0;
            if (_noleggio.IsCorpo == 1)
            {
                importo = (decimal)_noleggio.CostoCorpo + (decimal)_noleggio.AltriCostis.Sum(s => s.Importo);
            }
            else
            {
                importo = (decimal)(_noleggio.CostoMetroQuadro * (decimal)_noleggio.MetriQuadri) + +(decimal)_noleggio.AltriCostis.Sum(s => s.Importo); ;
            }

            decimal importoTotale = importo + Calcolaiva(importo);

            decimal importoBonifico = Decimal.Round((importoTotale / 100) * (Convert.ToInt32(txtPercentualeBonifico.Text)), 2);
            decimal residuo = importoTotale - importoBonifico;
            decimal importoPrimaRiba = Decimal.Round(residuo / 3, 2);
            decimal importoSecondaRiba = Decimal.Round(residuo / 3, 2);
            decimal ImportoUltimaRiba = residuo - importoPrimaRiba - importoSecondaRiba;

            var pagamento = new Pagamento();
            pagamento.idPagamento = Guid.NewGuid();
            pagamento.idNoleggio = _noleggio.idNoleggio;
            pagamento.Importo = importoBonifico;
            pagamento.ImportoIva = ScorporaIva(importoBonifico);
            pagamento.Descrizione = "Bonifico Bancario 30%";
            pagamento.DataScadenzaPagamento = _noleggio.dataComincioNoleggio;
            _ctx.Pagamentos.InsertOnSubmit(pagamento);
            _ctx.SubmitChanges();

            pagamento = new Pagamento();
            pagamento.idPagamento = Guid.NewGuid();
            pagamento.idNoleggio = _noleggio.idNoleggio;
            pagamento.Importo = importoPrimaRiba;
            pagamento.ImportoIva = ScorporaIva(importoPrimaRiba);

            DateTime today = (DateTime)_noleggio.dataComincioNoleggio;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            pagamento.DataScadenzaPagamento = endOfMonth.AddMonths(1);
            pagamento.Descrizione = "Scadenza Ri.Ba. " + String.Format("{0:dd/MM/yyyy}", pagamento.DataScadenzaPagamento);
            _ctx.Pagamentos.InsertOnSubmit(pagamento);
            _ctx.SubmitChanges();

            pagamento = new Pagamento();
            pagamento.idPagamento = Guid.NewGuid();
            pagamento.idNoleggio = _noleggio.idNoleggio;
            pagamento.Importo = importoSecondaRiba;
            pagamento.ImportoIva = ScorporaIva(importoSecondaRiba);
            pagamento.Descrizione = "";
            pagamento.DataScadenzaPagamento = endOfMonth.AddMonths(2);
            pagamento.Descrizione = "Scadenza Ri.Ba. " + String.Format("{0:dd/MM/yyyy}", pagamento.DataScadenzaPagamento);
            _ctx.Pagamentos.InsertOnSubmit(pagamento);
            _ctx.SubmitChanges();

            pagamento = new Pagamento();
            pagamento.idPagamento = Guid.NewGuid();
            pagamento.idNoleggio = _noleggio.idNoleggio;
            pagamento.Importo = ImportoUltimaRiba;
            pagamento.ImportoIva = ScorporaIva(ImportoUltimaRiba);
            pagamento.Descrizione = "";
            pagamento.DataScadenzaPagamento = endOfMonth.AddMonths(3);
            pagamento.Descrizione = "Scadenza Ri.Ba. " + String.Format("{0:dd/MM/yyyy}", pagamento.DataScadenzaPagamento);
            _ctx.Pagamentos.InsertOnSubmit(pagamento);

            _noleggio.DescrizioneNoleggio = "Bonifico Bancario " + txtPercentualeBonifico.Text + "% \rRi.ba 30/60/90 gg d.f. f.m.";
            _ctx.SubmitChanges();

            RicaricaNoleggio(_noleggio.idNoleggio);




        }

        private decimal ScorporaIva(decimal importo)
        {
            return importo - (importo / (decimal)(1 + _noleggio.IVA / 100));
        }

        private decimal Calcolaiva(decimal importo)
        {
            return (importo / 100) * (decimal)_noleggio.IVA;
        }

        private void imgAggiungiNotaDiCredito_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var win = new NuovaNotaDiCredito(_noleggio);
            try
            {
                win.ShowDialog();
            }
            catch (Exception)
            {


            }
            var id = _noleggio.idNoleggio;
            _ctx = new AlianteLinqDataContext();
            _noleggio = _ctx.Noleggios.Where(s => s.idNoleggio == id).First();
            RicaricaNoleggio(id);
        }

        private void btnEditNotaDiCredito_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var id = btn.CommandParameter.ToString();
            var guid = new Guid(id);
            var newW = new NuovaNotaDiCredito(guid, _noleggio);
            newW.ShowDialog();
            RicaricaNoleggio(_noleggio.idNoleggio);
        }

        private void btnDeleteNotaDiCredito_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Vuoi Eliminare questa nota di credito?", "Confirmation", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var btn = (Button)sender;
                var id = btn.CommandParameter.ToString();
                var guid = new Guid(id);
                UtilityNoleggio.EliminaNotaDiCredito(guid);
                RicaricaNoleggio(_noleggio.idNoleggio);
            }
        }

        private void imgPdfFatturaNotaDiCredito_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var id = ((Fattura)((Image)sender).DataContext).IdFattura;
            Fattura fatt = new Fattura();

            foreach (var item in dgFattureNotaDiCredito.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    fatt = (Fattura)item;
            }

            fatt.DataNotaDiCredito = fatt.DataFattura;

            var notaDiCredito = _ctx.NotaCreditos.Where(s => s.idFattura == fatt.IdFattura).First();

            if (_ctx.PagamentoProrogas.Where(s => s.idNotadiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var pr = _ctx.PagamentoProrogas.Where(s => s.idNotadiCredito == notaDiCredito.idNotaCredito).First();
                pr.DataNotaDiCredito = fatt.DataNotaDiCredito;
            }
            if (_ctx.Interventos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var interv = _ctx.Interventos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).First();
                interv.DataNotaDiCredito = fatt.DataNotaDiCredito;
            }
            if (_ctx.Pagamentos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var pag = _ctx.Pagamentos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito);
                foreach (var p in pag)
                {
                    p.DataNotaDiCredito = fatt.DataNotaDiCredito;
                }
            }

            _ctx.SubmitChanges();


            if (fatt.PdfFattura == null || fattureDaRicaricare.Contains(fatt))
            {
                fatt.PdfFattura = GestioneReport.GeneraNotaDiCredito(fatt); ;
                _ctx.SubmitChanges();
            }

            GestioneReport.MostraPdf(fatt.PdfFattura.ToArray());
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            var id = ((Fattura)((TextBox)sender).DataContext).IdFattura;
            foreach (var item in dgFattureNotaDiCredito.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    if (!string.IsNullOrEmpty(((TextBox)sender).Text))
                        ((Fattura)item).NumeroFattura = Convert.ToInt32(((TextBox)sender).Text);
            }
        }

        private void DatePicker_SelectedDateChanged_6(object sender, SelectionChangedEventArgs e)
        {
            var id = ((Fattura)((DatePicker)sender).DataContext).IdFattura;
            foreach (var item in dgFattureNotaDiCredito.Items)
            {
                if (((Fattura)item).IdFattura == id)
                    ((Fattura)item).DataFattura = ((DatePicker)sender).SelectedDate;
            }
        }

        private void imgPdfDdt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var camions = new List<Camion>();
            foreach (var item in lbCamion.SelectedItems)
            {
                camions.Add((Camion)item);
            }

            GestioneReport.MostraPdf(GestioneReport.GeneraDdt(_noleggio, camions));
        }

        private void txtQuantita_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (txt.Text != "0" && !String.IsNullOrEmpty(txt.Text))
            {
                if (dgMateriali.SelectedCells.Count > 0)
                {
                    var item = dgMateriali.SelectedCells[0].Item;
                    System.Type type = item.GetType();
                    Guid idMateriale = (Guid)type.GetProperty("idMateriale").GetValue(item, null);
                    if (_ctx.MaterialeNoleggios.Where(s => s.idMateriale == idMateriale && s.idNoleggio == _noleggio.idNoleggio).Count() > 0)
                        _ctx.MaterialeNoleggios.Where(s => s.idMateriale == idMateriale && s.idNoleggio == _noleggio.idNoleggio).First().Quantità = Convert.ToInt32(txt.Text);
                    else
                    {
                        var materiale = new MaterialeNoleggio();
                        materiale.idMateriale = idMateriale;
                        materiale.idNoleggio = _noleggio.idNoleggio;
                        materiale.Quantità = Convert.ToInt32(txt.Text);
                        _ctx.MaterialeNoleggios.InsertOnSubmit(materiale);
                    }
                    _ctx.SubmitChanges();
                }
            }
        }

        private void CheckBoxZone_Checked(object sender, RoutedEventArgs e)
        {
            var cbx = ((CheckBox)sender);
            var item = (Camion)cbx.DataContext;
            if ((bool)cbx.IsChecked)
                lbCamion.SelectedItems.Add(item);
            else
                lbCamion.SelectedItems.Remove(item);
        }

        private void auto_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Pagamento)((CheckBox)sender).DataContext).Bonifico = vOut;

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var banca = (Banca)(((ComboBox)sender).SelectedItem);
            if (banca != null)
            {
                var idBanca = banca.idBanca;
                ((Pagamento)((ComboBox)sender).DataContext).idBanca = idBanca;
            }
        }

        private void cbBonificoProproga_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Proroga)((CheckBox)sender).DataContext).Bonifico = vOut;
        }

        private void cbBonificoIntervento_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            short vOut = Convert.ToInt16(isChecked);
            ((Intervento)((CheckBox)sender).DataContext).Bonifico = vOut;
        }

        private void cmbBanchePror_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var banca = (Banca)(((ComboBox)sender).SelectedItem);
            if (banca != null)
            {
                var idBanca = banca.idBanca;
                ((Proroga)((ComboBox)sender).DataContext).idBanca = idBanca;
            }
        }

        private void cmbBancheInt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var banca = (Banca)(((ComboBox)sender).SelectedItem);
            if (banca != null)
            {
                var idBanca = banca.idBanca;
                ((Intervento)((ComboBox)sender).DataContext).idBanca = idBanca;
            }
        }

        private void auto_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            if (isChecked)
            {
                var win = new DataPagamento();
                win.ShowDialog();
                if (win.DataPagamentoScelta != null)
                    ((Pagamento)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
                _ctx.SubmitChanges();
            }

        }

        private void auto_Click_1(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            if (isChecked)
            {
                var win = new DataPagamento();
                win.ShowDialog();
                if (win.DataPagamentoScelta != null)
                    ((Proroga)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
                _ctx.SubmitChanges();
            }
        }

        private void auto_Click_2(object sender, RoutedEventArgs e)
        {
            bool isChecked = (bool)((CheckBox)sender).IsChecked;
            if (isChecked)
            {
                var win = new DataPagamento();
                win.ShowDialog();
                if (win.DataPagamentoScelta != null)
                    ((Intervento)((CheckBox)sender).DataContext).DataPagamento = win.DataPagamentoScelta;
                _ctx.SubmitChanges();
            }
        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var id = btn.CommandParameter.ToString();
            var guid = new Guid(id);
            var newW = new PagamentiIntervento(guid);
            newW.ShowDialog();
            RicaricaNoleggio(_noleggio.idNoleggio);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ((Pagamento)((DatePicker)sender).DataContext).DataAcconto = ((DatePicker)sender).SelectedDate;
        }
    }
}
