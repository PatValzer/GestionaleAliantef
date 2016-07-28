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
    /// Interaction logic for Mastrino.xaml
    /// </summary>
    public partial class Mastrino : Window
    {

        #region VARIABILI GLOBALI
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        bool inizializzato = false;


        List<vwFatture> _fattureScadute;
        List<vwFatture> _fattureFuture;


        #endregion

        public Mastrino()
        {
            InitializeComponent();

            CaricaComboCliente();

            _fattureScadute = _ctx.vwFattures.Where(s => (s.DataScadenzaPagamento <= DateTime.Today && s.DataScadenzaPagamento != null)).ToList();
            _fattureFuture = _ctx.vwFattures.Where(s => (s.DataScadenzaPagamento > DateTime.Today && s.DataScadenzaPagamento != null)).ToList();

            RicaricaMastrino();
            inizializzato = true;



        }

        private void RicaricaLista()
        {

            var ricerca = txtRicerca.Text.ToUpper();
            var fatture = new List<vwFatture>();
            var fattureConNotaDicredito = new List<vwFatture>();
            var fattureScadute = _fattureScadute.Where(s => s.RagioneSociale.ToUpper().Contains(ricerca) || s.NumeroFattura.ToString().ToUpper().Contains(ricerca)).ToList() ;
            var fattureFuture = _fattureFuture.Where(s => s.RagioneSociale.ToUpper().Contains(ricerca) || s.NumeroFattura.ToString().ToUpper().Contains(ricerca)).ToList(); 

            if (dpDal.SelectedDate != null)
                fattureScadute = fattureScadute.Where(s => s.DataFattura > dpDal.SelectedDate).ToList();

            if (dpAl.SelectedDate != null)
                fattureScadute = fattureScadute.Where(s => s.DataFattura < dpAl.SelectedDate).ToList();

            if (dpDalScadenza.SelectedDate != null)
                fattureScadute = fattureScadute.Where(s => s.DataScadenzaPagamento > dpDalScadenza.SelectedDate).ToList();

            if (dpAlScadenza.SelectedDate != null)
                fattureScadute = fattureScadute.Where(s => s.DataScadenzaPagamento < dpAlScadenza.SelectedDate).ToList();


            if (dpDal.SelectedDate != null)
                fattureFuture = fattureFuture.Where(s => s.DataFattura > dpDal.SelectedDate).ToList();

            if (dpAl.SelectedDate != null)
                fattureFuture = fattureFuture.Where(s => s.DataFattura < dpAl.SelectedDate).ToList();

            if (dpDalScadenza.SelectedDate != null)
                fattureFuture = fattureFuture.Where(s => s.DataScadenzaPagamento > dpDalScadenza.SelectedDate).ToList();

            if (dpAlScadenza.SelectedDate != null)
                fattureFuture = fattureFuture.Where(s => s.DataScadenzaPagamento < dpAlScadenza.SelectedDate).ToList();


            if (cmbNodalitaPagamento.SelectedIndex == 2)
            {

                fattureFuture = fattureFuture.Where(s => s.Bonifico == 1).ToList();
                fattureScadute = fattureScadute.Where(s => s.Bonifico == 1).ToList();
            }

            if (cmbNodalitaPagamento.SelectedIndex == 1)
            {

                fattureFuture = fattureFuture.Where(s => s.Bonifico != 1).ToList();
                fattureScadute = fattureScadute.Where(s => s.Bonifico != 1).ToList();
            }

            decimal totaleFattureScadute = 0;
            decimal totaleFattureInsolute = 0;
            decimal totaleFatturePagate = 0;
            decimal totaleNoteDiCredito = 0;
            decimal totaleFattureFuture = 0;


            decimal totaleFattureScaduteIVA = 0;
            decimal totaleFattureInsoluteIVA = 0;
            decimal totaleFatturePagateIVA = 0;
            decimal totaleNoteDiCreditoIVA = 0;
            decimal totaleFattureFutureIVA = 0;

            if ((bool)cbxPagato.IsChecked)
            {

                var fatturePagate = fattureScadute.Where(s => (s.Pagato == 1)).ToList();
                foreach (var fatt in fatturePagate)
                {
                    if (fatt.ImportoNotaDiCredito > 0)
                    {
                        if (!(bool)cbxNoteDiCredito.IsChecked)
                        {
                            totaleFatturePagate -= (decimal)fatt.ImportoNotaDiCredito;
                            var importoTemp = fatt.ImportoNotaDiCredito - (fatt.ImportoNotaDiCredito / (decimal)(1 + fatt.IVA / 100));
                            totaleFatturePagateIVA -= (decimal)importoTemp;
                        }
                    }

                }


                totaleFatturePagate += (decimal)fatturePagate.Sum(s => s.Importo);
                totaleFatturePagateIVA += (decimal)fatturePagate.Sum(s => s.ImportoIva);
                fatture.AddRange(fatturePagate);
            }



            if ((bool)cbxInsoluto.IsChecked)
            {
                var fattureInsolute = fattureScadute.Where(s => (s.Pagato != 1 || s.Pagato == null) && s.Insoluto == 1).ToList();


                foreach (var fatt in fattureInsolute)
                {
                    //totaleFattureInsolute += (decimal)fatt.Importo;

                    if (fatt.ImportoNotaDiCredito > 0)
                    {

                        if (!(bool)cbxNoteDiCredito.IsChecked)
                        {
                            totaleFattureInsolute -= (decimal)fatt.ImportoNotaDiCredito;
                            var importoTemp = fatt.ImportoNotaDiCredito - (fatt.ImportoNotaDiCredito / (decimal)(1 + fatt.IVA / 100));
                            totaleFattureInsoluteIVA -= (decimal)importoTemp;
                        }
                        //totaleFattureInsolute -= (decimal)fatt.ImportoNotaDiCredito;


                    }
                }

                totaleFattureInsolute += (decimal)fattureInsolute.Sum(s => s.Importo);
                totaleFattureInsoluteIVA += (decimal)fattureInsolute.Sum(s => s.ImportoIva);
                fatture.AddRange(fattureInsolute);
            }
            if ((bool)cbxScaduto.IsChecked)
            {
                var fattureScaduteNonInsolute = fattureScadute.Where(s => (s.Pagato != 1 || s.Pagato == null) && (s.Insoluto != 1 || s.Insoluto == null)).ToList();


                foreach (var fatt in fattureScaduteNonInsolute)
                {
                    if (fatt.ImportoNotaDiCredito > 0)
                    {
                        if (!(bool)cbxNoteDiCredito.IsChecked)
                        {

                            totaleFattureScadute -= (decimal)fatt.ImportoNotaDiCredito;
                            var importoTemp = fatt.ImportoNotaDiCredito - (fatt.ImportoNotaDiCredito / (decimal)(1 + fatt.IVA / 100));
                            totaleFattureScaduteIVA -= (decimal)importoTemp;
                        
                        }

                    }
                }


                totaleFattureScadute += (decimal)fattureScaduteNonInsolute.Sum(s => s.Importo);
                totaleFattureScaduteIVA += (decimal)fattureScaduteNonInsolute.Sum(s => s.ImportoIva);
                fatture.AddRange(fattureScaduteNonInsolute);
            }
            if ((bool)cbxFuturo.IsChecked)
            {

                foreach (var fatt in fattureFuture)
                {

                    if (fatt.ImportoNotaDiCredito > 0)
                    {

                        if (!(bool)cbxNoteDiCredito.IsChecked)
                        {
                            totaleFattureFuture -= (decimal)fatt.ImportoNotaDiCredito;
                            var importoTemp = fatt.ImportoNotaDiCredito - (fatt.ImportoNotaDiCredito / (decimal)(1 + fatt.IVA / 100));
                            totaleFattureFutureIVA -= (decimal)importoTemp;
                        }

                    }
                }



                totaleFattureFuture += (decimal)fattureFuture.Sum(s => s.Importo);
                totaleFattureFutureIVA += (decimal)fattureFuture.Sum(s => s.ImportoIva);
                fatture.AddRange(fattureFuture);
            }

            if ((bool)cbxNoteDiCredito.IsChecked)
            {
                //var fattureFuture = _ctx.vwFattures.Where(s => s.DataScadenzaPagamento > DateTime.Today && s.DataScadenzaPagamento != null && (s.Pagato != 1 || s.Pagato == null) && (s.Insoluto != 1 || s.Insoluto == null));
                fattureConNotaDicredito = _ctx.vwFattures.Where(s => s.TipoFattura == 4).ToList();
                fatture.AddRange(fattureConNotaDicredito);
                //lblNdc.Content = ((decimal)noteDiCredito.Sum(s => s.Importo));

            }
            else
            {
                foreach (var ndc in fattureConNotaDicredito)
                {

                    var noteDiCredito = _ctx.NotaCreditos.Where(s => s.idNotaCredito == ndc.idNotaDiCredito).First();

                    fatture.AddRange(_ctx.vwFattures.Where(s => s.IdFattura == noteDiCredito.idFattura));
                }
            }

            if (cmbNodalitaPagamento.SelectedIndex == 2)
            {

                fatture = fatture.Where(s => s.Bonifico == 1).ToList();
            }

            if (cmbNodalitaPagamento.SelectedIndex == 1)
            {

                fatture = fatture.Where(s => s.Bonifico != 1).ToList();
            }



            fatture.AddRange(_ctx.vwFattures.Where(s => s.DataScadenzaPagamento == null && (s.TipoFattura == null || s.TipoFattura != 4)));

            if (dpDal.SelectedDate != null)
                fatture = fatture.Where(s => s.DataFattura > dpDal.SelectedDate).ToList();

            if (dpAl.SelectedDate != null)
                fatture = fatture.Where(s => s.DataFattura < dpAl.SelectedDate).ToList();

            if (dpDalScadenza.SelectedDate != null)
                fatture = fatture.Where(s => s.DataScadenzaPagamento > dpDalScadenza.SelectedDate).ToList();

            if (dpAlScadenza.SelectedDate != null)
                fatture = fatture.Where(s => s.DataScadenzaPagamento < dpAlScadenza.SelectedDate).ToList();


            fatture = fatture.OrderByDescending(s => s.DataFattura).ThenByDescending(s => s.DataScadenzaPagamento).Where(s => s.RagioneSociale.ToUpper().Contains(ricerca) || s.NumeroFattura.ToString().Contains(ricerca)).ToList();


            totaleNoteDiCredito = (decimal)fatture.Where(s => s.TipoFattura == 4).Sum(s => s.Importo);
            totaleNoteDiCreditoIVA = (decimal)fatture.Where(s => s.TipoFattura == 4).Sum(s => s.ImportoIva);



            lblPagati.Content = (totaleFatturePagate).ToString("c") + "(" + (totaleFatturePagateIVA).ToString("c") + ")";
            lblInsoluti.Content = (totaleFattureInsolute).ToString("c") + "(" + (totaleFattureInsoluteIVA).ToString("c") + ")";
            lblScaduti.Content = (totaleFattureScadute).ToString("c") + "(" + (totaleFattureScaduteIVA).ToString("c") + ")";
            lblFuturi.Content = (totaleFattureFuture).ToString("c") + "(" + (totaleFattureFutureIVA).ToString("c") + ")";
            lblTotali.Content = (totaleFattureInsolute + totaleFattureFuture + totaleFattureScadute + totaleNoteDiCredito).ToString("c") + "(" + (totaleFatturePagateIVA + totaleFattureInsoluteIVA + totaleFattureFutureIVA + totaleFattureScaduteIVA + totaleNoteDiCreditoIVA).ToString("c") + ")";
            lblNdc.Content = (totaleNoteDiCredito).ToString("c") + "(" + (totaleNoteDiCreditoIVA).ToString("c") + ")";

            dgFatture.ItemsSource = fatture;

           
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            RicaricaLista();
        }

        private void dgFatture_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var fattura = (vwFatture)e.Row.DataContext;
            if (fattura.Pagato == 1)
                e.Row.Foreground = new SolidColorBrush(Colors.Green);
            else
                if (fattura.Insoluto == 1)
                    e.Row.Foreground = new SolidColorBrush(Colors.Orange);
                else
                {
                    if (fattura.DataScadenzaPagamento < DateTime.Now)
                        e.Row.Foreground = new SolidColorBrush(Colors.Red);
                    else
                    {
                        var col = (Color)ColorConverter.ConvertFromString("#FF575656");
                        e.Row.Foreground = new SolidColorBrush(col);
                    }
                }
        }

        private void cbxInsoluto_Checked(object sender, RoutedEventArgs e)
        {
            if (inizializzato)
            {
               //_fattureScadute = _ctx.vwFattures.Where(s => (s.DataScadenzaPagamento <= DateTime.Today && s.DataScadenzaPagamento != null)).ToList();
               // _fattureFuture = _ctx.vwFattures.Where(s => (s.DataScadenzaPagamento > DateTime.Today && s.DataScadenzaPagamento != null)).ToList();

                RicaricaLista();
            }
        }

        private void AButton_Click_1(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var id = btn.CommandParameter.ToString();
            var guid = new Guid(id);
            var newW = new GestioneNoleggio(guid);
            newW.ShowDialog();
            RicaricaMastrino();
        }

        private void imgPdfFatturaProroga_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (((vwFatture)((Image)sender).DataContext).PdfFattura != null)
                GestioneReport.MostraPdf(((vwFatture)((Image)sender).DataContext).PdfFattura.ToArray());
        }

        private void dpAl_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            RicaricaLista();
        }

        private void cmbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem != null)
            {
                RicaricaAlbero(sender);
            }
        }

        private void RicaricaAlbero(object sender)
        {
            if (((ComboBox)sender).SelectedItem != null)
            {
                var id = ((Cliente)(((ComboBox)sender).SelectedItem)).IdCliente;
                var cliente = _ctx.Clientes.Where(s => s.IdCliente == id).First();
                var noleggi = _ctx.Noleggios.Where(s => s.idCliente == cliente.IdCliente);
                tvSituazioneCliente.Items.Clear();


                decimal totaleImportiCliente = 0;
                decimal totaleInsolutiCliente = 0;
                decimal totalePagatiCliente = 0;

                List<Indirizzo> indirizzi = new List<Indirizzo>();
                foreach (var nol in noleggi)
                {
                    //var name = nol.idNoleggio.ToString();


                    var countInterventi = _ctx.Interventos.Where(s => s.idNoleggio == nol.idNoleggio && (s.Pagato != 1 || s.Pagato == null)).Count();
                    var countProroghe = _ctx.Prorogas.Where(s => s.idNoleggio == nol.idNoleggio && (s.Pagato != 1 || s.Pagato == null)).Count();
                    var countPagamenti = _ctx.Pagamentos.Where(s => s.idNoleggio == nol.idNoleggio && (s.Pagato != 1 || s.Pagato == null)).Count();
                    decimal noteDiCreditoEvase = 0;

                    var PagamentiPagatiImporto = nol.Pagamentos.Where(s => s.Pagato == 1).Sum(s => s.Importo) + nol.Pagamentos.Where(s => s.Pagato != 1 && s.ImportoAcconto > 0).Sum(s => s.ImportoAcconto);
                    var PagamentiImporto = nol.Pagamentos.Sum(s => s.Importo);
                    decimal noteDiCreditoEvasePagamenti = 0;
                    foreach (var pag in nol.Pagamentos.Where(s => s.idNotaDiCredito != null && (s.Pagato != 1 || s.Pagato == null)))
                    {
                        var importoNota = _ctx.NotaCreditos.Where(s => s.idNotaCredito == pag.idNotaDiCredito).First().ImportoTotaleIvato;
                        noteDiCreditoEvase += (decimal)importoNota;
                        noteDiCreditoEvasePagamenti += (decimal)importoNota;
                    }
                    var PagamentiInsoluti = PagamentiImporto - PagamentiPagatiImporto - noteDiCreditoEvasePagamenti;

                    decimal noteDiCreditoEvaseProroghe = 0;
                    decimal ProroghePagatiImporto = 0;
                    decimal ProrogheImporto = 0;
                    foreach (var pror in nol.Prorogas)
                    {
                        ProroghePagatiImporto += (decimal)pror.PagamentoProrogas.Where(s => s.Pagato == 1).Sum(s => s.Importo);
                        ProrogheImporto += (decimal)pror.PagamentoProrogas.Sum(s => s.Importo);
                    }
                    // = nol.Prorogas.Where(s => s.Pagato == 1).Sum(s => s.ImportoTotaleIvato);
                    //var ProrogheImporto = nol.Prorogas.Sum(s => s.ImportoTotaleIvato);

                    foreach (var pag in nol.Prorogas.Where(s => s.idNotaDiCredito != null && (s.Pagato != 1 || s.Pagato == null)))
                    {
                        if (_ctx.NotaCreditos.Where(s => s.idNotaCredito == pag.idNotaDiCredito).Count() > 0)
                        {
                            var importoNota = _ctx.NotaCreditos.Where(s => s.idNotaCredito == pag.idNotaDiCredito).First().ImportoTotaleIvato;
                            noteDiCreditoEvase += (decimal)importoNota;
                            noteDiCreditoEvaseProroghe += (decimal)importoNota;
                        }
                    }
                    var ProrogheInsoluti = ProrogheImporto - ProroghePagatiImporto - noteDiCreditoEvaseProroghe;


                    decimal noteDiCreditoEvaseInterventi = 0;
                    var InterventiPagatiImporto = nol.Interventos.Where(s => s.Pagato == 1).Sum(s => s.ImportoTotaleIvato);
                    var InterventiImporto = nol.Interventos.Sum(s => s.ImportoTotaleIvato);

                    foreach (var pag in nol.Interventos.Where(s => s.idNotaDiCredito != null && (s.Pagato != 1 || s.Pagato == null)))
                    {
                        var importoNota = _ctx.NotaCreditos.Where(s => s.idNotaCredito == pag.idNotaDiCredito).First().ImportoTotaleIvato;
                        noteDiCreditoEvase += (decimal)importoNota;
                        noteDiCreditoEvaseInterventi += (decimal)importoNota;
                    }
                    var InterventiInsoluti = InterventiImporto - InterventiPagatiImporto - noteDiCreditoEvaseInterventi;
                    var NotediCreditoImporto = nol.NotaCreditos.Sum(s => s.ImportoTotaleIvato);

                    var totalePagatiImporto = PagamentiPagatiImporto + ProroghePagatiImporto + InterventiPagatiImporto;
                    var totaleImporto = PagamentiImporto + ProrogheImporto + InterventiImporto - NotediCreditoImporto;
                    var totaleInsoluti = PagamentiInsoluti + ProrogheInsoluti + InterventiInsoluti;


                    totaleImportiCliente += (decimal)totaleImporto;
                    totaleInsolutiCliente += (decimal)totaleInsoluti;
                    totalePagatiCliente += (decimal)totalePagatiImporto;

                    //var pagato = (countPagamenti == 0) && (countProroghe == 0) && (countInterventi == 0);

                    var indirizzo = nol.Indirizzo.Via + ", " + nol.Indirizzo.NumeroCivico + " " + nol.Indirizzo.Citta;
                    while (indirizzo.Length < 29)
                        indirizzo = indirizzo + " ";

                    var totaleRicevuti = " RICEVUTI:" + ((decimal)totalePagatiImporto).ToString("c");
                    while (totaleRicevuti.Length < 25)
                        totaleRicevuti = totaleRicevuti + " ";
                    var totaleInAttesa = "IN ATTESA:" + ((decimal)totaleInsoluti).ToString("c");
                    while (totaleInAttesa.Length < 25)
                        totaleInAttesa = totaleInAttesa + " ";

                    //NODO PADRE
                    var item = new TreeViewItem() { Header = indirizzo + "\t" + totaleRicevuti + "\t" + totaleInAttesa + "\t" + " TOTALE:" + ((decimal)totaleImporto).ToString("c") };
                    item.Foreground = (totaleImporto == 0) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red); ;
                    item.FontWeight = FontWeights.Bold;
                    item.DataContext = nol;
                    item.MouseDoubleClick += item_MouseDoubleClick;
                    item.MouseRightButtonDown += item_MouseRightButtonDown;

                    //NODO PRIMA FATTURA
                    var totalePagamentiRicevuti = " RICEVUTI:" + ((decimal)PagamentiPagatiImporto).ToString("c");
                    while (totalePagamentiRicevuti.Length < 25)
                        totalePagamentiRicevuti = totalePagamentiRicevuti + " ";
                    var totalePagamentiInAttesa = "  IN ATTESA:" + ((decimal)PagamentiInsoluti).ToString("c");
                    while (totalePagamentiInAttesa.Length < 29)
                        totalePagamentiInAttesa = totalePagamentiInAttesa + " ";



                    var itemFattura = new TreeViewItem() { Header = "Prima Fattura" + "\t\t\t    " + totalePagamentiRicevuti + totalePagamentiInAttesa + " TOTALE:" + ((decimal)PagamentiImporto).ToString("c") };
                    itemFattura.FontWeight = FontWeights.Normal;
                    itemFattura.Foreground = countPagamenti == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);


                    foreach (var pag in _ctx.Pagamentos.Where(s => s.idNoleggio == nol.idNoleggio).OrderByDescending(s => s.DataScadenzaPagamento))
                    {
                        var itemPagamento = new TreeViewItem() { Header = string.IsNullOrEmpty(pag.Descrizione) ? "" : pag.Descrizione + "  -\t" + ((decimal)pag.Importo).ToString("c") };
                        itemPagamento.Foreground = pag.Pagato == 1 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                        itemPagamento.FontWeight = FontWeights.Light;
                        itemPagamento.FontStyle = FontStyles.Italic;

                        itemPagamento.MouseDoubleClick += itemPagamento_MouseDoubleClick;
                        itemPagamento.DataContext = pag;
                        itemFattura.Items.Add(itemPagamento);
                    }


                    //NODO PROROGHE
                    var totaleProrogheRicevuti = " RICEVUTI:" + ((decimal)ProroghePagatiImporto).ToString("c");
                    while (totaleProrogheRicevuti.Length < 25)
                        totaleProrogheRicevuti = totaleProrogheRicevuti + " ";

                    var totaleProrogheInAttesa = "  IN ATTESA:" + ((decimal)ProrogheInsoluti).ToString("c");
                    while (totaleProrogheInAttesa.Length < 29)
                        totaleProrogheInAttesa = totaleProrogheInAttesa + " ";


                    var itemProroghe = new TreeViewItem() { Header = "Proroghe" + "\t\t\t    " + totaleProrogheRicevuti + totaleProrogheInAttesa + " TOTALE:" + ((decimal)ProrogheImporto).ToString("c") };
                    itemProroghe.Foreground = countProroghe == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    itemProroghe.FontWeight = FontWeights.Normal;

                    foreach (var pag in _ctx.Prorogas.Where(s => s.idNoleggio == nol.idNoleggio).OrderByDescending(s => s.DataScadenzaPagamento))
                    {
                        var itemProroga = new TreeViewItem() { Header = pag.Descrizione + "  -\t" + ((decimal)pag.ImportoTotaleIvato).ToString("c") };
                        itemProroga.Foreground = pag.Pagato == 1 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                        itemProroga.FontWeight = FontWeights.Light;
                        itemProroga.FontStyle = FontStyles.Italic;
                        itemProroga.MouseDoubleClick += itemProroga_MouseLeftButtonDown;
                        itemProroga.DataContext = pag;
                        itemProroghe.Items.Add(itemProroga);
                    }

                    //NODO INTERVENTI
                    var totaleInterventiRicevuti = " RICEVUTI:" + ((decimal)InterventiPagatiImporto).ToString("c");
                    while (totaleInterventiRicevuti.Length < 25)
                        totaleInterventiRicevuti = totaleInterventiRicevuti + " ";

                    var totaleInterventiInAttesa = "  IN ATTESA:" + ((decimal)InterventiInsoluti).ToString("c");
                    while (totaleInterventiInAttesa.Length < 29)
                        totaleInterventiInAttesa = totaleInterventiInAttesa + " ";

                    var itemInterventi = new TreeViewItem() { Header = "Interventi" + "\t\t\t    " + totaleInterventiRicevuti + totaleInterventiInAttesa + " TOTALE:" + ((decimal)InterventiImporto).ToString("c") };
                    itemInterventi.Foreground = countInterventi == 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    itemInterventi.FontWeight = FontWeights.Normal;

                    foreach (var pag in _ctx.Interventos.Where(s => s.idNoleggio == nol.idNoleggio).OrderByDescending(s => s.DataScadenzaPagamento))
                    {
                        var itemIntervento = new TreeViewItem() { Header = pag.Descrizione + "  -\t" + ((decimal)pag.ImportoTotaleIvato).ToString("c") };
                        itemIntervento.Foreground = pag.Pagato == 1 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                        itemIntervento.FontWeight = FontWeights.Light;
                        itemIntervento.FontStyle = FontStyles.Italic;
                        itemIntervento.MouseDoubleClick += itemIntervento_MouseDoubleClick;
                        itemIntervento.DataContext = pag;
                        itemInterventi.Items.Add(itemIntervento);
                    }

                    //NODO NOTEDICREDITO
                    var totaleNoteDiCredito = " EMESSE:" + ((decimal)NotediCreditoImporto).ToString("c");
                    while (totaleProrogheRicevuti.Length < 25)
                        totaleProrogheRicevuti = totaleProrogheRicevuti + " ";




                    var itemNoteDiCredito = new TreeViewItem() { Header = "Note di Credito" + "\t\t\t\t\t\t\t\t\t\t     TOTALE: " + ((decimal)-NotediCreditoImporto).ToString("c") };
                    itemNoteDiCredito.Foreground = new SolidColorBrush(Colors.Red);
                    itemNoteDiCredito.FontWeight = FontWeights.Normal;
                    itemNoteDiCredito.FontStyle = FontStyles.Normal;

                    foreach (var pag in _ctx.NotaCreditos.Where(s => s.idNoleggio == nol.idNoleggio))
                    {
                        var itemProroga = new TreeViewItem() { Header = pag.Descrizione + "  -\t" + ((decimal)pag.ImportoTotaleIvato).ToString("c") };
                        itemProroga.Foreground = new SolidColorBrush(Colors.Red);
                        itemProroga.FontWeight = FontWeights.Light;
                        itemProroga.FontStyle = FontStyles.Italic;
                        itemProroga.MouseDoubleClick += itemProroga_MouseLeftButtonDown;
                        itemProroga.DataContext = pag;
                        itemNoteDiCredito.Items.Add(itemProroga);
                    }

                    item.Items.Add(itemFattura);
                    if (_ctx.Prorogas.Where(s => s.idNoleggio == nol.idNoleggio).Count() > 0)
                        item.Items.Add(itemProroghe);
                    if (_ctx.Interventos.Where(s => s.idNoleggio == nol.idNoleggio).Count() > 0)
                        item.Items.Add(itemInterventi);
                    if (_ctx.NotaCreditos.Where(s => s.idNoleggio == nol.idNoleggio).Count() > 0)
                        item.Items.Add(itemNoteDiCredito);

                    tvSituazioneCliente.Items.Add(item);
                }

                lblTotaleCliente.Content = totaleImportiCliente.ToString("c");
                lblRicevutiCliente.Content = totalePagatiCliente.ToString("c");
                lblInsolutiCliente.Content = totaleInsolutiCliente.ToString("c");
            }
        }

        void item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var wnd = new GeneraMastrino((Noleggio)((TreeViewItem)sender).DataContext);
            wnd.ShowDialog();
        }

        void item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var noleggio = (Noleggio)((TreeViewItem)sender).DataContext;
            var wnd = new GestioneNoleggio(noleggio.idNoleggio);
            wnd.ShowDialog();
            RicaricaMastrino();
        }

        private void RicaricaMastrino()
        {
            _ctx = new AlianteLinqDataContext();
            RicaricaLista();
            RicaricaAlbero(cmbCliente);
        }

        void itemIntervento_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var intervento = (Intervento)((TreeViewItem)sender).DataContext;
            if (intervento.Fattura != null)
                GestioneReport.MostraPdf(intervento.Fattura.PdfFattura.ToArray());
        }

        void itemPagamento_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pagamento = (Pagamento)((TreeViewItem)sender).DataContext;
            var noleggio = new AlianteLinqDataContext().Noleggios.Where(s => s.idNoleggio == pagamento.idNoleggio).First();
            if (noleggio.Fattura != null)
                GestioneReport.MostraPdf(noleggio.Fattura.PdfFattura.ToArray());
        }

        void itemProroga_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var proroga = (Proroga)((TreeViewItem)sender).DataContext;
            if (proroga.Fattura != null)
                GestioneReport.MostraPdf(proroga.Fattura.PdfFattura.ToArray());
        }

        private void CaricaComboCliente()
        {
            cmbCliente.ItemsSource = new AlianteLinqDataContext().Clientes.OrderBy(s => s.RagioneSociale);
            cmbCliente.DisplayMemberPath = "RagioneSociale";
        }

        int printcount = 0;


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
 
            var fatture = (List<vwFatture>)dgFatture.ItemsSource;
            var acconti = _ctx.vwFattures.Where(s => s.RagioneSociale.ToUpper().Contains(txtRicerca.Text.ToUpper()) && s.ImportoAcconto > 0 && (s.TipoFattura != 4 || s.TipoFattura == null) && s.Pagato != 1).ToList();

            foreach (var acconto in acconti)
            {
                acconto.Importo = -acconto.ImportoAcconto;
            }

            if (dpDal.SelectedDate != null)
                acconti = acconti.Where(s => s.DataFattura > dpDal.SelectedDate).ToList();

            if (dpAl.SelectedDate != null)
                acconti = acconti.Where(s => s.DataFattura < dpAl.SelectedDate).ToList();

            if (dpDalScadenza.SelectedDate != null)
                acconti = acconti.Where(s => s.DataScadenzaPagamento > dpDalScadenza.SelectedDate).ToList();

            if (dpAlScadenza.SelectedDate != null)
                acconti = acconti.Where(s => s.DataScadenzaPagamento < dpAlScadenza.SelectedDate).ToList();

            fatture.AddRange(acconti);
            GestioneReport.MostraExcel(GestioneReport.GeneraDatiMastrino((fatture)));

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RicaricaLista();
        }

    }


}
