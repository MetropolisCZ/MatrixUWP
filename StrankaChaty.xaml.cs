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
        private string pristupovyToken = ApplicationData.Current.LocalSettings.Values["pristupovyToken"]?.ToString();
        private string uzivatelskeJmeno = ApplicationData.Current.LocalSettings.Values["uzivatelskeJmeno"]?.ToString();
        private string matrixServer = ApplicationData.Current.LocalSettings.Values["MatrixServer"]?.ToString();

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
                string UrlKziskani = "https://" + matrixServer + "/_matrix/client/r0/sync";
                var aaa = await NacistStrankuRestApi(UrlKziskani);
                MatrixSyncOdpoved matrixSyncOdpoved = JsonConvert.DeserializeObject<MatrixSyncOdpoved>(aaa);
                MatrixSeznamChatu.Clear();

                foreach (var jedenChatMatrix in matrixSyncOdpoved.Rooms.Join)
                {
                    JObject mBridgeChannelContent = (JObject)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.bridge" && e.Content != null)?.LastOrDefault()?.Content?["channel"]);
                    string mRoomAvatarContent = (string)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.room.avatar" && e.Content != null)?.LastOrDefault()?.Content?["url"]) ?? null;
                    BitmapImage obrazekChatu = new BitmapImage();
                    if (mRoomAvatarContent != null)
                    {
                        IBuffer BufferObrazku = await NacistBufferRestApi("https://" + matrixServer + "/_matrix/client/v1/media/download/" + mRoomAvatarContent.Remove(0, 6));

                        InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
                        DataWriter writer = new DataWriter(memoryStream);
                        writer.WriteBuffer(BufferObrazku);
                        await writer.StoreAsync();
                        memoryStream.Seek(0);

                        await obrazekChatu.SetSourceAsync(memoryStream);
                    }
                    else
                    { // Obrázek neni
                    }
                    

                    string nazevChatu =
                        jedenChatMatrix.Value.Timeline?.Events?.Where(e => e.Type == "m.room.name" && e.Content?.Name != null)?.LastOrDefault()?.Content?.Name
                        ?? mBridgeChannelContent?["displayname"]?.ToString()
                        ?? jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.room.member" && e.Content != null && e.Content.TryGetValue("displayname", out object value) && value?.ToString() != uzivatelskeJmeno && value?.ToString().Contains("bridge bot") == false)?.LastOrDefault()?.Content["displayname"].ToString()
                        ?? "ID " + jedenChatMatrix.Key;

                    MatrixSeznamChatu.Add(new MatrixSeznamChatu_JedenChat
                    {
                        IdChatu = jedenChatMatrix.Key,
                        NazevChatu = nazevChatu,
                        PosledniZprava = jedenChatMatrix.Value.Timeline?.Events?.Where(e => e.Type == "m.room.message" && e.Content?.Body != null)?.LastOrDefault()?.Content?.Body ?? "Obsah nebyl nalezen",
                        UnixoveSekundyPosledniZpravy = jedenChatMatrix.Value.Timeline?.Events?.LastOrDefault()?.OriginServerTs ?? 0,
                        ObrazekChatu = obrazekChatu
                        //new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(jedenChatMatrix.Value.Timeline?.Events?.LastOrDefault()?.OriginServerTs ?? 0.0)
                    });
                }

                MatrixSeznamChatu.Sort((x, y) => y.UnixoveSekundyPosledniZpravy.CompareTo(x.UnixoveSekundyPosledniZpravy));

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
