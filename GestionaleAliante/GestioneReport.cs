
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace GestionaleAliante
{
    public static class GestioneReport
    {
        public static byte[] GeneraFattura(Noleggio noleggio, bool bonifico)
        {
            byte[] bytes = null;
            LocalReport lr = new LocalReport();
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            lr.ReportPath = @"Fattura.rdlc";



            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Cliente.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("DatiFattura", ctx.vwGeneraFatturas.Where(s => s.idNoleggio == noleggio.idNoleggio)));
            lr.DataSources.Add(new ReportDataSource("Pagamento", ctx.Pagamentos.Where(s => s.idNoleggio == noleggio.idNoleggio).OrderBy(t => t.DataScadenzaPagamento)));
            lr.DataSources.Add(new ReportDataSource("AltriCosti", ctx.AltriCostis.Where(s => s.idNoleggio == noleggio.idNoleggio)));
            if (!bonifico)
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.Cliente.idBanca)));
            else
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));
            lr.DataSources.Add(new ReportDataSource("BancaNoleggio", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));

            var parameters = new ReportParameterCollection();
            parameters.Add(new ReportParameter("idNoleggio", noleggio.idNoleggio.ToString()));
            parameters.Add(new ReportParameter("bonifico", bonifico.ToString()));

            try
            {
                lr.SetParameters(parameters);
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }

        public static byte[] GeneraFatturaProroga(Fattura fattura, bool bonifico)
        {
            byte[] bytes = null;


            LocalReport lr = new LocalReport();
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var noleggio = ctx.Noleggios.Where(s => s.idNoleggio == ctx.Prorogas.Where(t => t.idFattura == fattura.IdFattura).First().idNoleggio).First();


            lr.ReportPath = @"FatturaProroga.rdlc";



            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Cliente.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Cliente", ctx.Clientes.Where(s => s.IdCliente == noleggio.idCliente)));
            lr.DataSources.Add(new ReportDataSource("Noleggio", ctx.Noleggios.Where(s => s.idNoleggio == noleggio.idNoleggio)));
            lr.DataSources.Add(new ReportDataSource("Proroga", ctx.Prorogas.Where(t => t.idFattura == fattura.IdFattura)));
            lr.DataSources.Add(new ReportDataSource("Fattura", ctx.Fatturas.Where(t => t.IdFattura == fattura.IdFattura)));
            if (!bonifico)
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.Cliente.idBanca)));
            else
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));
            lr.DataSources.Add(new ReportDataSource("BancaNoleggio", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoNoleggio", ctx.Indirizzos.Where(t => t.IdIndirizzo == noleggio.idIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Pagamenti", ctx.PagamentoProrogas.Where(t => t.idProroga == ctx.Prorogas.Where(r => r.idFattura == fattura.IdFattura).First().IdProrogra)));


            var parameters = new ReportParameterCollection();
            parameters.Add(new ReportParameter("bonifico", bonifico.ToString()));

            try
            {
                lr.SetParameters(parameters);
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }

        public static void MostraPdf(byte[] datiDaVisualizzare)
        {
            if (!Directory.Exists(Config.cartellaSalvataggi))
                Directory.CreateDirectory(Config.cartellaSalvataggi);
            var fileName = Config.cartellaSalvataggi + Guid.NewGuid() + ".pdf";
            File.WriteAllBytes(fileName, datiDaVisualizzare);
            var p = new System.Diagnostics.Process();
            var t = new System.Diagnostics.ProcessStartInfo(fileName);
            t.UseShellExecute = true;
            t.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo = t;
            p.Start();
        }

        public static void MostraExcel(byte[] datiDaVisualizzare)
        {
            if (!Directory.Exists(Config.cartellaSalvataggi))
                Directory.CreateDirectory(Config.cartellaSalvataggi);
            var fileName = Config.cartellaSalvataggi + Guid.NewGuid() + ".xls";
            File.WriteAllBytes(fileName, datiDaVisualizzare);
            var p = new System.Diagnostics.Process();
            var t = new System.Diagnostics.ProcessStartInfo(fileName);
            t.UseShellExecute = true;
            t.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo = t;
            p.Start();
        }

        public static byte[] GeneraFatturaIntervento(Fattura fattura, bool bonifico)
        {
            byte[] bytes = null;


            LocalReport lr = new LocalReport();
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var noleggio = ctx.Noleggios.Where(s => s.idNoleggio == ctx.Interventos.Where(t => t.idFattura == fattura.IdFattura).First().idNoleggio).First();


            lr.ReportPath = @"FatturaIntervento.rdlc";



            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Cliente.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Cliente", ctx.Clientes.Where(s => s.IdCliente == noleggio.idCliente)));
            lr.DataSources.Add(new ReportDataSource("Noleggio", ctx.Noleggios.Where(s => s.idNoleggio == noleggio.idNoleggio)));
            lr.DataSources.Add(new ReportDataSource("Intervento", ctx.Interventos.Where(t => t.idFattura == fattura.IdFattura)));
            lr.DataSources.Add(new ReportDataSource("Fattura", ctx.Fatturas.Where(t => t.IdFattura == fattura.IdFattura)));
            lr.DataSources.Add(new ReportDataSource("Pagamenti", ctx.PagamentoInterventos.Where(t => t.idIntervento == ctx.Interventos.Where(r => r.idFattura == fattura.IdFattura).First().idIntervento)));

            if (!bonifico)
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.Cliente.idBanca)));
            else
                lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));

            lr.DataSources.Add(new ReportDataSource("BancaNoleggio", ctx.Bancas.Where(t => t.idBanca == noleggio.idBanca)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoNoleggio", ctx.Indirizzos.Where(t => t.IdIndirizzo == noleggio.idIndirizzo)));

            var parameters = new ReportParameterCollection();
            parameters.Add(new ReportParameter("bonifico", bonifico.ToString()));

            try
            {
                lr.SetParameters(parameters);
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }

        public static byte[] GeneraMastrino(Noleggio noleggio, DateTime? dal, DateTime? al, bool totaleCliente)
        {
            byte[] bytes = null;



            LocalReport lr = new LocalReport();
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();


            lr.ReportPath = @"Mastrino.rdlc";

            List<vwFatture> fatture = new List<vwFatture>();
            List<vwFatture> fatturePagate = new List<vwFatture>();
            List<vwFatture> acconti = new List<vwFatture>();



            if (totaleCliente)
            {
                fatture = ctx.vwFattures.Where(s => s.idCliente == noleggio.idCliente && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
                fatturePagate = ctx.vwFattures.Where(s => s.idCliente == noleggio.idCliente && s.Pagato == 1 && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
                acconti = ctx.vwFattures.Where(s => s.idCliente == noleggio.idCliente && s.ImportoAcconto > 0 && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
            }
            else
            {
                fatture = ctx.vwFattures.Where(s => s.idNoleggio == noleggio.idNoleggio && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
                fatturePagate = ctx.vwFattures.Where(s => s.idNoleggio == noleggio.idNoleggio && s.Pagato == 1 && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
                acconti = ctx.vwFattures.Where(s => s.idNoleggio == noleggio.idNoleggio && s.idCliente == noleggio.idCliente && s.ImportoAcconto > 0 && (s.TipoFattura != 4 || s.TipoFattura == null)).ToList();
            }

            foreach (var f in acconti.Where(s => s.ImportoNotaDiCredito > 0))
            {
                f.ImportoNotaDiCredito = null;
            }


            if (dal != null)
            {
                fatture = fatture.Where(s => s.DataFattura >= dal || s.DataPagamento >= dal).ToList();
                fatturePagate = fatturePagate.Where(s => s.DataPagamento >= dal).ToList();
                acconti = acconti.Where(s => s.DataPagamento >= dal).ToList();

                foreach (var f in fatture.Where(s => s.ImportoNotaDiCredito > 0))
                {
                    if (f.DataNotaDiCredito < dal)
                        f.ImportoNotaDiCredito = null;
                }

                foreach (var f in fatturePagate.Where(s => s.ImportoNotaDiCredito > 0))
                {
                    if (f.DataNotaDiCredito < dal)
                        f.ImportoNotaDiCredito = null;
                }

                //foreach (var f in acconti.Where(s => s.ImportoNotaDiCredito > 0))
                //{
                //    if (f.DataNotaDiCredito < dal)
                //        f.ImportoNotaDiCredito = null;
                //}
            }

            if (al != null)
            {
                fatture = fatture.Where(s => s.DataFattura <= al || s.DataPagamento <= al).ToList();
                fatturePagate = fatturePagate.Where(s => s.DataPagamento <= al).ToList();
                acconti = acconti.Where(s => s.DataPagamento <= dal).ToList();

                foreach (var f in fatture.Where(s => s.ImportoNotaDiCredito > 0))
                {
                    if (f.DataNotaDiCredito > al)
                        f.ImportoNotaDiCredito = null;
                }

                foreach (var f in fatturePagate.Where(s => s.ImportoNotaDiCredito > 0))
                {
                    if (f.DataNotaDiCredito > al)
                        f.ImportoNotaDiCredito = null;
                }

                //foreach (var f in acconti.Where(s => s.ImportoNotaDiCredito > 0))
                //{
                //    if (f.DataNotaDiCredito > al)
                //        f.ImportoNotaDiCredito = null;
                //}
            }

            


            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Cliente.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Cliente", ctx.Clientes.Where(s => s.IdCliente == noleggio.idCliente)));
            
            lr.DataSources.Add(new ReportDataSource("FatturePagate", fatturePagate.OrderBy(s => s.DataFattura).ThenBy(s => s.DataScadenzaPagamento)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoNoleggio", ctx.Indirizzos.Where(t => t.IdIndirizzo == noleggio.idIndirizzo)));

            var parameters = new ReportParameterCollection();
            //var val =  fatture.Where(s => s.Insoluto == 1).Sum(s => s.Importo).ToString();
            var fattureInsolute = fatture.Where(s => s.Insoluto == 1);
            var importoInsoluti = fattureInsolute.Sum(s => s.Importo);
            var importoNoteDiCreditoFattureInsolute = fattureInsolute.Sum(s => s.ImportoNotaDiCredito);

            var val = (importoInsoluti - (importoNoteDiCreditoFattureInsolute == null ? 0 : importoNoteDiCreditoFattureInsolute)).ToString();


            var insoluti = new ReportParameter("totaleInsoluti", val == "" ? "0" : val);
            var dalFiltro = new ReportParameter("dal", dal == null ? "01/01/1900" : dal.ToString());
            var allFiltro = new ReportParameter("al", al == null ? "01/01/2200" : al.ToString());

            var importoNoteDiCreditoFatturePagate = fatturePagate.Sum(s => s.ImportoNotaDiCredito).ToString();
            var noteDiCreditoPagate = new ReportParameter("totaleNoteDiCreditoPagate", importoNoteDiCreditoFatturePagate == "" ? "0" : importoNoteDiCreditoFatturePagate);
            var totale = new ReportParameter("totaleCliente", totaleCliente.ToString());

            decimal pagamentiEmessi = (decimal)fatture.Sum(s => s.Importo);

            if (al != null)
            {
                pagamentiEmessi = (decimal)fatture.Where(s => s.DataFattura <= al).Sum(s => s.Importo);
            }

            if (dal != null)
            {
                pagamentiEmessi = (decimal)fatture.Where(s => s.DataFattura >= dal).Sum(s => s.Importo);
            }

            for (int i = 0; i < fatturePagate.Count; i++)
            {
                fatturePagate[i].DataOperazione = fatturePagate[i].DataPagamento;
                fatturePagate[i].Descrizione = "INCASSO FATTURA";
                fatturePagate[i].Importo = fatturePagate[i].Importo - (fatturePagate[i].ImportoAcconto > 0 ? fatturePagate[i].ImportoAcconto : 0);
            }

            decimal pagamentiRicevuti = (decimal)fatturePagate.Sum(s => s.Importo);

            if (al != null)
            {
                pagamentiRicevuti = (decimal)fatturePagate.Where(s => s.DataPagamento <= al).Sum(s => s.Importo);
            }

            if (dal != null)
            {
                pagamentiRicevuti = (decimal)fatturePagate.Where(s => s.DataPagamento >= dal).Sum(s => s.Importo);
            }


            decimal accontiRicevuti = (decimal)acconti.Sum(s => s.ImportoAcconto);

            if (al != null)
            {
                accontiRicevuti = (decimal)acconti.Where(s => s.DataAcconto <= al).Sum(s => s.ImportoAcconto);
            }

            if (dal != null)
            {
                accontiRicevuti = (decimal)acconti.Where(s => s.DataAcconto >= dal).Sum(s => s.ImportoAcconto);
            }




            parameters.Add(insoluti);
            parameters.Add(noteDiCreditoPagate);
            parameters.Add(totale);
            parameters.Add(dalFiltro);
            parameters.Add(allFiltro);
            parameters.Add(new ReportParameter("pagamentiEmessi", pagamentiEmessi.ToString()));
            parameters.Add(new ReportParameter("pagamentiRicevuti", pagamentiRicevuti.ToString()));
            parameters.Add(new ReportParameter("accontiRicevuti", accontiRicevuti.ToString()));


            for (int i = 0; i < acconti.Count; i++)
            {
                acconti[i].DataOperazione = acconti[i].DataAcconto;
                acconti[i].Importo = acconti[i].ImportoAcconto;
                acconti[i].Descrizione = "ACCONTO";
            }

            for (int i = 0; i < fatture.Count; i++)
            {
                fatture[i].DataOperazione = fatture[i].DataFattura;
            }

            fatture.AddRange(fatturePagate);
            fatture.AddRange(acconti);
            lr.DataSources.Add(new ReportDataSource("Fatture", fatture.OrderBy(s => s.DataOperazione).ThenBy(s => s.DataScadenzaPagamento)));
            
            try
            {
                lr.SetParameters(parameters);
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }

            ctx.Dispose();
            return bytes;
        }

        public static byte[] GeneraNotaDiCredito(Fattura fattura)
        {
            byte[] bytes = null;


            LocalReport lr = new LocalReport();
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var noleggio = ctx.Noleggios.Where(s => s.idNoleggio == ctx.NotaCreditos.Where(t => t.idFattura == fattura.IdFattura).First().idNoleggio).First();


            if (noleggio.Prorogas.Where(s => s.idNotaDiCredito == fattura.NotaCreditos.First().idNotaCredito).Count() > 0)
            {
                var pr = noleggio.Prorogas.Where(s => s.idNotaDiCredito == fattura.NotaCreditos.First().idNotaCredito).First();
                var numeroFattura = fattura.NumeroFattura.ToString();
                while (numeroFattura.Length < 3)
                    numeroFattura += " ";
                pr.DescrizioneNotaDiCredito = "n. " + numeroFattura + " del " + String.Format("{0:dd/MM/yyyy}", fattura.DataFattura);

            }
            if (noleggio.Interventos.Where(s => s.idNotaDiCredito == fattura.NotaCreditos.First().idNotaCredito).Count() > 0)
            {
                var interv = noleggio.Interventos.Where(s => s.idNotaDiCredito == fattura.NotaCreditos.First().idNotaCredito).First();
                var numeroFattura = fattura.NumeroFattura.ToString();
                while (numeroFattura.Length < 3)
                    numeroFattura += " ";
                interv.DescrizioneNotaDiCredito = "n. " + numeroFattura + " del " + String.Format("{0:dd/MM/yyyy}", fattura.DataFattura);
            }
            if (noleggio.Pagamentos.Where(s => s.idNotaDiCredito == fattura.NotaCreditos.First().idNotaCredito).Count() > 0)
            {
                var pag = noleggio.Pagamentos;
                var numeroFattura = fattura.NumeroFattura.ToString();
                while (numeroFattura.Length < 3)
                    numeroFattura += " ";
                foreach (var p in pag)
                    p.DescrizioneNotaDiCredito = "n. " + numeroFattura + " del " + String.Format("{0:dd/MM/yyyy}", fattura.DataFattura);
            }
            ctx.SubmitChanges();

            lr.ReportPath = @"NotaDiCredito.rdlc";



            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Cliente.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Cliente", ctx.Clientes.Where(s => s.IdCliente == noleggio.idCliente)));
            lr.DataSources.Add(new ReportDataSource("Noleggio", ctx.Noleggios.Where(s => s.idNoleggio == noleggio.idNoleggio)));
            lr.DataSources.Add(new ReportDataSource("NotaDiCredito", ctx.NotaCreditos.Where(t => t.idFattura == fattura.IdFattura)));
            lr.DataSources.Add(new ReportDataSource("Fattura", ctx.Fatturas.Where(t => t.IdFattura == fattura.IdFattura)));
            lr.DataSources.Add(new ReportDataSource("BancaCliente", ctx.Bancas.Where(t => t.idBanca == noleggio.Cliente.idBanca)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoNoleggio", ctx.Indirizzos.Where(t => t.IdIndirizzo == noleggio.idIndirizzo)));

            try
            {
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }

        public static byte[] GeneraDdt(Noleggio noleggio, List<Camion> camions)
        {
            byte[] bytes = null;


            LocalReport lr = new LocalReport();
            var ctx = new AlianteLinqDataContext();
            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", ctx.Indirizzos.Where(s => s.IdIndirizzo == noleggio.Indirizzo.IdIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("Cliente", ctx.Clientes.Where(s => s.IdCliente == noleggio.Cliente.IdCliente)));
            lr.DataSources.Add(new ReportDataSource("Camion", camions));
            var result = from t in ctx.Materiales
                         join x in ctx.MaterialeNoleggios.Where(s => s.idNoleggio == noleggio.idNoleggio) on t.idMateriale equals x.idMateriale
                         into JoinedEmpDept
                         from dept in JoinedEmpDept.DefaultIfEmpty()
                         orderby t.Ordine
                         select new
                         {
                             DescrizioneMateriale = t.DescrizioneMateriale,
                             Quantità = dept != null ? dept.Quantità : 0
                         };

            lr.DataSources.Add(new ReportDataSource("Materiali", result));
            lr.ReportPath = @"Ddt.rdlc";

            try
            {
                bytes = lr.Render("Pdf");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Errore durante la generazione del Ddt \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }



        public static byte[] GeneraDatiMastrino(List<vwFatture> fatture)
        {
            byte[] bytes = null;


            LocalReport lr = new LocalReport();
            var ctx = new AlianteLinqDataContext();


            lr.DataSources.Add(new ReportDataSource("Fatture", fatture.OrderBy(s => s.DataScadenzaPagamento)));
            lr.ReportPath = @"DatiMastrino.rdlc";

            try
            {
                bytes = lr.Render("Excel");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Errore durante la generazione del Ddt \rRicontrollare i campi");
                throw ex;
            }


            return bytes;
        }

        public static byte[] PathReport(object value, string idBanca, string idCliente, string idIndirizzo)
        {
            //string returnValue = "";

            Warning[] warnings = null;
            string[] streamids = null;
            var mimeType = "";
            var encoding = "";



            Microsoft.Reporting.WebForms.LocalReport lr = new Microsoft.Reporting.WebForms.LocalReport();

            string deviceInfo =
              "<DeviceInfo>" +
              "<SimplePageHeaders>True</SimplePageHeaders>" +
              "</DeviceInfo>";
            string pippo = "";
            lr.ReportPath = @"NotaSpese.rdlc";

            AlianteLinqDataContext db = new AlianteLinqDataContext();



            Noleggio nol = db.Noleggios.Where(s => s.idNoleggio.ToString() == value.ToString()).First();



            lr.DataSources.Add(new ReportDataSource("DataSet2", db.Noleggios.Where(s => s.idNoleggio.ToString() == value.ToString())));
            lr.DataSources.Add(new ReportDataSource("DataSet1", nol.Pagamentos.OrderBy(s => s.DataScadenzaPagamento)));
            lr.DataSources.Add(new ReportDataSource("AltriCosti", nol.AltriCostis));
            lr.DataSources.Add(new ReportDataSource("Cliente", db.Clientes.Where(s => s.IdCliente == nol.idCliente)));
            lr.DataSources.Add(new ReportDataSource("Banca", db.Bancas.Where(s => s.idBanca.ToString() == idBanca)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoCantiere", db.Indirizzos.Where(s => s.IdIndirizzo == nol.idIndirizzo)));
            lr.DataSources.Add(new ReportDataSource("IndirizzoCliente", db.Indirizzos.Where(s => s.IdIndirizzo == nol.Cliente.idIndirizzo)));

            var parameters = new ReportParameterCollection();
            parameters.Add(new ReportParameter("idNoleggio", value.ToString()));
            parameters.Add(new ReportParameter("idIndirizzo", idIndirizzo.ToString()));
            parameters.Add(new ReportParameter("idIndirizzoCantiere", idIndirizzo.ToString()));
            parameters.Add(new ReportParameter("idIndirizzoCliente", idIndirizzo.ToString()));
            parameters.Add(new ReportParameter("idCliente", idCliente.ToString()));
            parameters.Add(new ReportParameter("idBanca", idBanca.ToString()));
            try
            {
                lr.SetParameters(parameters);
                byte[] bytes = lr.Render("Pdf", deviceInfo, out mimeType, out encoding, out pippo, out streamids, out warnings);
                return bytes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante la generazione della fattura \rRicontrollare i campi");
                throw ex;
            }
        }
    }
}
