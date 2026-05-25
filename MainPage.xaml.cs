using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static MatrixUWP.MatrixDatabazeObjekty;

// Dokumentaci k šabloně položky Prázdná stránka najdete na adrese https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x405

namespace MatrixUWP
{

    public sealed partial class MainPage : Page
    {

        public static Frame ContentFrame;
        public static TextBlock PageHeader;


        public MainPage()
        {
            InitializeComponent();

            //var testUdalost = new MatrixDatabaze_Udalost
            //{
            //    IdUdalosti = Guid.NewGuid().ToString(),
            //    IdMistnosti = "!testroom:server.cz",
            //    Odesilatel = "@tomas:server.cz",
            //    CasoveRazitko = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            //    Druh = "m.room.message",
            //    ObsahJSON = "{\"msgtype\":\"m.text\",\"body\":\"Testovací zpráva\"}",
            //    IndexVMistnosti = 1
            //};

            //MatrixDatabaze.Instance.VlozitUdalostDoDatabaze(testUdalost);

            ContentFrame = NavigacniRamec;
            PageHeader = NadpisStrankyTextBlock;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            if (ApplicationData.Current.LocalSettings.Values["pristupovyToken"] != null)
            {
                NavigovatNaStranku(typeof(StrankaChaty));
            }
            else
            {
                bool zobrazitPrihlaseniAutomaticky = true;
                NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
            }
        }

        public static void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                e.Handled = true;
            }
        }

        private void NavigacniRamec_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void TlacitkoUcet_Click(object sender, RoutedEventArgs e)
        {
            // Header dělá přímo ta stránka
            NavigovatNaStranku(typeof(StrankaNastaveni));
        }

        public static void NavigovatNaStranku(Type strankaKamNavigovatType, object navigacniParametry = null)
        {
            int puvodniCacheSize = ContentFrame.CacheSize;
            ContentFrame.CacheSize = 0;
            ContentFrame.CacheSize = puvodniCacheSize;
            if (navigacniParametry == null)
            {
                ContentFrame.Navigate(strankaKamNavigovatType);
            }
            else
            {
                ContentFrame.Navigate(strankaKamNavigovatType, navigacniParametry);
            }
            ContentFrame.BackStack.Clear();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void TlacitkoChaty_Click(object sender, RoutedEventArgs e)
        {
            NavigovatNaStranku(typeof(StrankaChaty));
        }
    }
}
