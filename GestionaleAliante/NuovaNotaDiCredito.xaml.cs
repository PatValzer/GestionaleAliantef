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
    /// Interaction logic for NuovaProroga.xaml
    /// </summary>
    public partial class NuovaNotaDiCredito : Window
    {


        Noleggio _noleggio = new Noleggio();
        AlianteLinqDataContext _ctx = new AlianteLinqDataContext();
        NotaCredito _notaDiCredito = null;
        bool inizializza = true;

        public NuovaNotaDiCredito(Noleggio nol)
        {
            InitializeComponent();
            _noleggio = nol;

            CreaIntestazione();
            txtDescrizione.Text = "noleggio relativo al periodo";
            txtIva.Text = _noleggio.IVA.ToString();

            PopolaComboFatture();
            inizializza = false;
        }

        private void PopolaComboFatture()
        {
            //cmbNumeroFattura.DisplayMemberPath = "Numero";
            //foreach (var fattura in _ctx.vwFattures.Wherevar (s => (s.TipoFattura != 4 || s.TipoFattura == null) && s.idNoleggio == _noleggio.idNoleggio))
            //{
            //    if (fattura.NumeroFattura != null && fattura.DataFattura != null)
            //        if (!cmbNumeroFattura.Items.Contains(fattura.NumeroFattura.ToString() + "/" + string.Format("{0:yy}", fattura.DataFattura)))
            //        {
            //            var obj = new {Numero = fattura.NumeroFattura.ToString() + "/" + string.Format("{0:yy}", fattura.DataFattura), Data =   string.Format("{0:dd/MM/yyyy}", fattura.DataFattura)};

            //            cmbNumeroFattura.Items.Add(obj);
            //        }


            //}
            var fatture = new List<Fattura>();
            fatture.Add(_noleggio.Fattura);
            foreach (var intervento in _noleggio.Interventos)
                fatture.Add(intervento.Fattura);
            foreach (var proroga in _noleggio.Prorogas)
                fatture.Add(proroga.Fattura);
            cmbNumeroFattura.ItemsSource = fatture.OrderByDescending(s => s.DataFattura);
        }

        private void CreaIntestazione()
        {
            txtIntestazioneNotaDiCredito.Text = "";
            txtIntestazioneNotaDiCredito.Text += "Storno parziale della fattura nr. ";
            if (cmbNumeroFattura.SelectedValue != null && cmbNumeroFattura.SelectedValue.ToString() != "")
            {
                txtIntestazioneNotaDiCredito.Text += ((Fattura)cmbNumeroFattura.SelectedItem).NumeroFattura.ToString() + " / " + string.Format("{0:yy}", ((Fattura)cmbNumeroFattura.SelectedItem).DataFattura);
            }
            txtIntestazioneNotaDiCredito.Text += Environment.NewLine;
            //Inserisci Data Fattura
            if (cmbNumeroFattura.SelectedValue != null && cmbNumeroFattura.SelectedValue.ToString() != "")
            {
                txtIntestazioneNotaDiCredito.Text += "del " + string.Format("{0:dd/MM/yyyy}", ((Fattura)cmbNumeroFattura.SelectedItem).DataFattura) + " per lo smontaggio anticipato";
            }
            txtIntestazioneNotaDiCredito.Text += Environment.NewLine;
            txtIntestazioneNotaDiCredito.Text += "della copertura provvisoria installata presso:";
            txtIntestazioneNotaDiCredito.Text += Environment.NewLine;
            var indirizzo = _noleggio.Indirizzo;
            txtIntestazioneNotaDiCredito.Text += indirizzo.Via + ", " + indirizzo.NumeroCivico + " " + indirizzo.Citta;
            txtIntestazioneNotaDiCredito.Text += Environment.NewLine;
            txtIntestazioneNotaDiCredito.Text += "Montaggio ultimato e consegnato";
            txtIntestazioneNotaDiCredito.Text += Environment.NewLine;
            txtIntestazioneNotaDiCredito.Text += String.Format("{0:dd/MM/yyyy}", _noleggio.dataComincioNoleggio);
        }

        public NuovaNotaDiCredito(Guid idNotadiCredito, Noleggio nol)
        {
            InitializeComponent();
            _noleggio = nol;
            var notaDiCredito = _ctx.NotaCreditos.Where(s => s.idNotaCredito == idNotadiCredito).First();

            txtImporto.Text = notaDiCredito.Importo.ToString();
            txtIntestazioneNotaDiCredito.Text = notaDiCredito.Intestazione;
            txtDescrizione.Text = notaDiCredito.Descrizione;
            if (notaDiCredito.Iva != null)
                txtIva.Text = notaDiCredito.Iva.ToString();
            else
                txtIva.Text = _noleggio.IVA.ToString();

            PopolaComboFatture();
            //var num = notaDiCredito.Fattura.NumeroFattura.ToString() + "/" + string.Format("{o:yy}", notaDiCredito.Fattura.DataFattura);
            var fattSel = new Fattura();
            foreach (var it in cmbNumeroFattura.Items)
            {
                var fatturina = (Fattura)it;
                var str = fatturina.NumeroFattura + " / " + string.Format("{0:yy}", fatturina.DataFattura);
                if (str == notaDiCredito.NumeroFatturaDaStornare)
                    cmbNumeroFattura.SelectedItem = it;
            }

            _notaDiCredito = notaDiCredito;
            inizializza = false;
        }


        private void btnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (_notaDiCredito == null)
            {
                _notaDiCredito = new NotaCredito();
                _notaDiCredito.idNotaCredito = Guid.NewGuid();
                _ctx.NotaCreditos.InsertOnSubmit(_notaDiCredito);
                _ctx.Noleggios.Where(s => s.idNoleggio == _noleggio.idNoleggio).First().NotaCreditos.Add(_notaDiCredito);
                Fattura fatt = new Fattura();
                fatt.IdFattura = Guid.NewGuid();
                fatt.TipoFattura = 4;
                _notaDiCredito.Fattura = fatt;
            }

            _notaDiCredito.Importo = Convert.ToDecimal(txtImporto.Text);
            _notaDiCredito.Iva = Convert.ToInt32(txtIva.Text);
            _notaDiCredito.ImportoTotaleIvato = _notaDiCredito.Importo + (_notaDiCredito.Importo / 100) * (decimal)_notaDiCredito.Iva;
            _notaDiCredito.ImportoIva = (_notaDiCredito.Importo / 100) * (decimal)_notaDiCredito.Iva;
            _notaDiCredito.Descrizione = txtDescrizione.Text;
            _notaDiCredito.Intestazione = txtIntestazioneNotaDiCredito.Text;
            if (cmbNumeroFattura.SelectedValue != null && cmbNumeroFattura.SelectedValue.ToString() != "")
            {
                var fatt = (Fattura)cmbNumeroFattura.SelectedItem;
                if (fatt.Prorogas.Count() > 0)
                {
                    var pr = _ctx.Fatturas.Where(s => s.IdFattura == fatt.IdFattura).First().Prorogas.First().PagamentoProrogas.First();
                    pr.ImportoNotaDiCredito = _notaDiCredito.ImportoTotaleIvato;
                    pr.idNotadiCredito = _notaDiCredito.idNotaCredito;
                }
                if (fatt.Interventos.Count() > 0)
                {
                    var interv = _ctx.Fatturas.Where(s => s.IdFattura == fatt.IdFattura).First().Interventos.First().PagamentoInterventos.First();
                    interv.ImportoNotaDiCredito = _notaDiCredito.ImportoTotaleIvato;
                    interv.idNotadiCredito = _notaDiCredito.idNotaCredito;
                }
                if (fatt.Noleggios.Count() > 0)
                {
                    var pag = _ctx.Fatturas.Where(s => s.IdFattura == fatt.IdFattura).First().Noleggios.First().Pagamentos.OrderByDescending(s => s.DataScadenzaPagamento).First();
                    pag.ImportoNotaDiCredito = _notaDiCredito.ImportoTotaleIvato;
                    pag.idNotaDiCredito = _notaDiCredito.idNotaCredito;
                }
                _notaDiCredito.NumeroFatturaDaStornare = fatt.NumeroFattura.ToString() + " / " + String.Format("{0:yy}", fatt.DataFattura);
            }
            _ctx.SubmitChanges();
        }

        private void cmbNumeroFattura_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!inizializza)
            {
                MessageBoxResult result = MessageBox.Show("Attenzione! Verrà rigenerata l'intestazione!", "Confirmation", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    CreaIntestazione();
                }
            }
        }



    }
}
