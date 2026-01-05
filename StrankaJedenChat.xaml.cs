using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
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

        public StrankaJedenChat()
        {
            try { this.InitializeComponent(); } catch (Exception ex) { var dialog = new MessageDialog(ex.ToString()); _ = dialog.ShowAsync(); }
        }

        private async Task PrvotniNacteniChatu()
        {
            try
            {
                ObrazekKonverzace.ImageSource = chatKterySeMaZobrazit.ObrazekChatu;
                NazevKonverzace.Text = chatKterySeMaZobrazit.NazevChatu;

                string UrlNacistZpravyChatu = "https://" + StrankaChaty.matrixServer + "/_matrix/client/v3/rooms/" + chatKterySeMaZobrazit.IdChatu + "/messages?dir=b&limit=50";
                var aaa = await NacistStrankuRestApi(UrlNacistZpravyChatu);
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
