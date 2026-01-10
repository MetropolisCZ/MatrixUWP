using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static MatrixUWP.ApiWebKlient;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MatrixUWP
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaJedenChat : Page
    {

        private MatrixSeznamChatu_JedenChat chatKterySeMaZobrazit = new MatrixSeznamChatu_JedenChat();
        private ZpravyAktualniKonverzace zpravyAktualniKonverzace = new ZpravyAktualniKonverzace();

        public StrankaJedenChat()
        {
            InitializeComponent();
        }

        private async Task PrvotniNacteniChatu()
        {
            try
            {
                ObrazekKonverzace.ImageSource = chatKterySeMaZobrazit.ObrazekChatu;
                NazevKonverzace.Text = chatKterySeMaZobrazit.NazevChatu;

                StackPanelNacitani_Stav.Text = "Stahování synchronizačního souboru ze serveru";
                string UrlNacistZpravyChatu = "https://" + StrankaChaty.matrixServer + "/_matrix/client/v3/rooms/" + chatKterySeMaZobrazit.IdChatu + "/messages?dir=b&limit=50";
                var aaa = await NacistStrankuRestApi(UrlNacistZpravyChatu);

                StackPanelNacitani_Stav.Text = "Zpracovávání celkového synchronizačního souboru";
                zpravyAktualniKonverzace = JsonConvert.DeserializeObject<ZpravyAktualniKonverzace>(aaa);
                // Na indexu 0 je nejnovější zpráva (řazení od nejnovějších)

                StackPanelNacitani_Stav.Text = "Načítání souborů médií";
                foreach (Event JednaZpravaAktualniKonverzace in zpravyAktualniKonverzace.Zpravy)
                {
                    if (JednaZpravaAktualniKonverzace.Type == "m.room.message" && ZiskatHodnotuDictionary(JednaZpravaAktualniKonverzace.Content, "msgtype") == "m.image")
                    {
                        JednaZpravaAktualniKonverzace.ObrazekZpravy = await NacistMatrixObrazek(ZiskatHodnotuDictionary(JednaZpravaAktualniKonverzace.Content, "url")) ?? null;
                    }
                }


                StackPanelNacitani.Visibility = Visibility.Collapsed;
                ListViewZpravyChaty.Visibility = Visibility.Visible;
                ListViewZpravyChaty.ItemsSource = zpravyAktualniKonverzace.Zpravy.Reverse();

                //ListViewZpravyChaty.ScrollIntoView(zpravyAktualniKonverzace.Zpravy.LastOrDefault());

            }
            catch
            {
                _ = await new ContentDialog()
                {
                    Title = "Chyba při načítání nebo zpracovávání dat konverzace",
                    CloseButtonText = "Zavřít"
                }.ShowAsync();

                return;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = "Detail konverzace";

            try
            {
                chatKterySeMaZobrazit = (MatrixSeznamChatu_JedenChat)e.Parameter;
                await PrvotniNacteniChatu();
            }
            catch
            {
                _ = await new ContentDialog()
                {
                    Title = "Chyba při komunikaci mezi stránkami aplikace",
                    CloseButtonText = "Zavřít"
                }.ShowAsync();

                return;
            }

        }

    }
}
