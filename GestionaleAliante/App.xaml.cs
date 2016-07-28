using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace GestionaleAliante
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
 
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                

                AlianteLinqDataContext db = new AlianteLinqDataContext();
                if (!db.DatabaseExists())
                {
                    db.CreateDatabase();
                }

                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(
                        CultureInfo.CurrentCulture.IetfLanguageTag)));
                base.OnStartup(e);
            }
            catch (Exception EX)
            {

                MessageBox.Show(EX.Message);
            }
        }

        private void rowContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Application_Exit_1(object sender, ExitEventArgs e)
        {

            if (!Directory.Exists(Config.cartellaSalvataggi))
                Directory.CreateDirectory(Config.cartellaSalvataggi);
            var files = Directory.GetFiles(Config.cartellaSalvataggi);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                }
            }

        }

        private void Application_DispatcherUnhandledException_1(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            using (var sw = new StreamWriter("errori.txt", true))
            {

                sw.WriteLine(e.Exception.Message);
                sw.WriteLine(e.Exception.StackTrace);

            }
            // Prevent default unhandled exception processing
            e.Handled = true;
            MessageBox.Show(e.Exception.Message);
        }
    }
}
