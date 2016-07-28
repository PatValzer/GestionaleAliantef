using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;




namespace GestionaleAliante
{
    public class NullableValueConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return null;

            return value;
        }

        #endregion


    }


    public class AmountConverter : System.Windows.Data.IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var importo = values[0] == null ? 0 : System.Convert.ToDouble(values[0]);
            var importoNDC = values[1] == null ? 0 : System.Convert.ToDouble(values[1]);
            var imp = (importo - importoNDC);

            if (values.Count() > 2)
            {

                imp = imp - (imp / (1 + System.Convert.ToDouble(values[2]) / 100));
            }



            return imp.ToString("c");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    static class UtilityNoleggio
    {
        public static void EliminaNoleggio(Guid idNoleggio)
        {
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var noleggio = ctx.Noleggios.Where(s => s.idNoleggio == idNoleggio).First();
            ctx.AltriCostis.DeleteAllOnSubmit(ctx.AltriCostis.Where(s => s.idNoleggio == idNoleggio));
            ctx.Pagamentos.DeleteAllOnSubmit(ctx.Pagamentos.Where(s => s.idNoleggio == idNoleggio));
            ctx.Prorogas.DeleteAllOnSubmit(ctx.Prorogas.Where(s => s.idNoleggio == idNoleggio));
            ctx.Fatturas.DeleteAllOnSubmit(ctx.Fatturas.Where(s => s.IdFattura == noleggio.idFattura));
            ctx.Noleggios.DeleteOnSubmit(noleggio);
            ctx.SubmitChanges();
        }

        public static void EliminaProroga(Guid idProroga)
        {
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var proroga = ctx.Prorogas.Where(s => s.IdProrogra == idProroga).First();
            ctx.Fatturas.DeleteAllOnSubmit(ctx.Fatturas.Where(s => s.IdFattura == proroga.idFattura));
            ctx.PagamentoProrogas.DeleteAllOnSubmit(proroga.PagamentoProrogas);
            ctx.Prorogas.DeleteOnSubmit(proroga);
            ctx.SubmitChanges();
        }

        public static void EliminaNotaDiCredito(Guid idNotadiCredito)
        {
            AlianteLinqDataContext ctx = new AlianteLinqDataContext();
            var notaDiCredito = ctx.NotaCreditos.Where(s => s.idNotaCredito == idNotadiCredito).First();
            ctx.Fatturas.DeleteAllOnSubmit(ctx.Fatturas.Where(s => s.IdFattura == notaDiCredito.idFattura));


            if (ctx.PagamentoProrogas.Where(s => s.idNotadiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var pr = ctx.PagamentoProrogas.Where(s => s.idNotadiCredito == notaDiCredito.idNotaCredito).First();
                pr.ImportoNotaDiCredito = null;
                pr.idNotadiCredito = null;
                pr.DescrizioneNotaDiCredito = null;
            }
            if (ctx.Interventos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var interv = ctx.Interventos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).First();
                interv.ImportoNotaDiCredito = null;
                interv.idNotaDiCredito = null;
                interv.DescrizioneNotaDiCredito = null;
            }
            if (ctx.Pagamentos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito).Count() > 0)
            {
                var pag = ctx.Pagamentos.Where(s => s.idNotaDiCredito == notaDiCredito.idNotaCredito);
                foreach (var p in pag)
                {
                    p.ImportoNotaDiCredito = null;
                    p.idNotaDiCredito = null;
                    p.DescrizioneNotaDiCredito = null;
                }
                foreach (var p in pag.First().Noleggio.Pagamentos)
                    p.DescrizioneNotaDiCredito = null;
            }
            ctx.NotaCreditos.DeleteOnSubmit(notaDiCredito);
            ctx.SubmitChanges();
        }
    }

    public class Banche : List<Banca>
    {
        public Banche()
        {
            this.AddRange(new AlianteLinqDataContext().Bancas.Where(s => s.IsBancaAliante == 1).ToList());
        }
    }
}
