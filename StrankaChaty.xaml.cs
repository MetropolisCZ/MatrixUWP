using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static MatrixUWP.ApiWebKlient;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MatrixUWP
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaChaty : Page
    {

        public StrankaChaty()
        {
            InitializeComponent();

            DataContext = this;
            NacistChaty();

        }

        private async void NacistChaty()
        {
            try
            {
                //StackPanelNacitani_Stav.Text = "Řazení zpráv";

                //MatrixSluzbaSynchronizace.Instance.SeznamChatu .Sort((x, y) => y.UnixoveSekundyPosledniZpravy.CompareTo(x.UnixoveSekundyPosledniZpravy));

                StackPanelNacitani.Visibility = Visibility.Collapsed;
                ListViewChaty.Visibility = Visibility.Visible;

                ListViewChaty.ItemsSource = MatrixSluzbaSynchronizace.Instance.SeznamChatu;

            }
            catch (Exception e)
            {
                StackPanelNacitani_Stav.Text = "Chyba při načítání nebo zpracovávání dat";
                StackPanelNacitani_Kolecko.Visibility = Visibility.Collapsed;

                _ = await new ContentDialog()
                {
                    Title = "Chyba při načítání nebo zpracovávání dat",
                    Content = e,
                    CloseButtonText = "Zavřít"
                }.ShowAsync();

                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApplicationData.Current.LocalSettings.Values["pristupovyToken"] == null)
            {
                bool zobrazitPrihlaseniAutomaticky = true;
                MainPage.NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
            }

            MainPage.PageHeader.Text = "Všechny konverzace";

        }

        private void ListViewChaty_ItemClick(object sender, ItemClickEventArgs e)
        {
            MatrixSeznamChatu_JedenChat kliknutyChat = (MatrixSeznamChatu_JedenChat)e.ClickedItem;

            _ = MainPage.ContentFrame.Navigate(typeof(StrankaJedenChat), kliknutyChat);
        }
    }
}
