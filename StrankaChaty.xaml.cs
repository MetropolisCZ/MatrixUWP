using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        private string pristupovyToken = ApplicationData.Current.LocalSettings.Values["pristupovyToken"]?.ToString();
        private List<MatrixSeznamChatu_JedenChat> MatrixSeznamChatu = new List<MatrixSeznamChatu_JedenChat>();

        public StrankaChaty()
        {
            InitializeComponent();

            this.DataContext = this;

            var headers = httpClient.DefaultRequestHeaders;
            headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", pristupovyToken);
            NacistChaty();
        }

        private async void NacistChaty()
        {
            try
            {
                string UrlKziskani = "https://" + ApplicationData.Current.LocalSettings.Values["MatrixServer"]?.ToString() + "/_matrix/client/r0/sync";
                MatrixSyncOdpoved matrixSyncOdpoved = JsonConvert.DeserializeObject<MatrixSyncOdpoved>(await NacistStrankuRestApi(UrlKziskani));
                MatrixSeznamChatu.Clear();

                foreach (var jedenChatMatrix in matrixSyncOdpoved.Rooms.Join)
                {
                    MatrixSeznamChatu.Add(new MatrixSeznamChatu_JedenChat
                    {
                        IdChatu = jedenChatMatrix.Key,
                        NazevChatu = jedenChatMatrix.Value.Timeline?.Events?.Where(e => e.Type == "m.room.name" && e.Content?.Name != null)?.LastOrDefault()?.Content?.Name ?? jedenChatMatrix.Key,
                        PosledniZprava = jedenChatMatrix.Value.Timeline?.Events?.Where(e => e.Type == "m.room.message" && e.Content?.Body != null)?.LastOrDefault()?.Content?.Body ?? "Obsah nebyl nalezen"
                    });
                }

                ListViewChaty.ItemsSource = MatrixSeznamChatu;
            }
            catch
            {
                _ = await new ContentDialog()
                {
                    Title = "Chyba při zpracovávání dat",
                    CloseButtonText = "Zavřít"
                }.ShowAsync();

                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = "Chaty";

        }

        private void ListViewChaty_ItemClick(object sender, ItemClickEventArgs e)
        {
            MatrixSeznamChatu_JedenChat kliknutyChat = (MatrixSeznamChatu_JedenChat)e.ClickedItem;
        }
    }
}
