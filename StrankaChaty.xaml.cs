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
        private string pristupovyToken = ApplicationData.Current.LocalSettings.Values["pristupovyToken"]?.ToString();
        private string uzivatelskeJmeno = ApplicationData.Current.LocalSettings.Values["uzivatelskeJmeno"]?.ToString();
        private string matrixServer = ApplicationData.Current.LocalSettings.Values["MatrixServer"]?.ToString();

        private List<MatrixSeznamChatu_JedenChat> MatrixSeznamChatu = new List<MatrixSeznamChatu_JedenChat>();

        string ZiskatHodnotuDictionary(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
                return null;

            return dictionary.TryGetValue(key, out var obj)
                ? obj?.ToString()
                : null;

        }

        async Task<BitmapImage> NacistMatrixObrazek(string urlObrazku)
        {
            if (urlObrazku == null)
            {
                return null;
            }

            BitmapImage matrixObrazek = new BitmapImage();

            IBuffer BufferObrazku = await NacistBufferRestApi("https://" + matrixServer + "/_matrix/client/v1/media/download/" + urlObrazku.Remove(0, 6));

            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(memoryStream);
            writer.WriteBuffer(BufferObrazku);
            await writer.StoreAsync();
            memoryStream.Seek(0);

            await matrixObrazek.SetSourceAsync(memoryStream);

            return matrixObrazek;
        }


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
                string UrlSyncFiltrovana = "https://" + matrixServer + "/_matrix/client/r0/sync"; // ?filter={\"room\":{\"timeline\":{\"limit\":2},\"state\":{\"lazy_load_members\":true}}}
                var aaa = await NacistStrankuRestApi(UrlSyncFiltrovana);
                MatrixSyncOdpoved matrixSyncOdpoved = JsonConvert.DeserializeObject<MatrixSyncOdpoved>(aaa);
                MatrixSeznamChatu.Clear();

                foreach (var jedenChatMatrix in matrixSyncOdpoved.Rooms.Join)
                {
                    JObject mBridgeChannelContent = (JObject)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.bridge" && e.Content != null)?.LastOrDefault()?.Content?["channel"]);

                    string urlObrazkuChatu = (string)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.room.avatar" && e.Content != null)?.LastOrDefault()?.Content?["url"]) ?? null;


                    /*string nazevChatu =
						jedenChatMatrix.Value.Timeline?.Events?.Where(e => e.Type == "m.room.name" && e.Content?["name"] != null)?.LastOrDefault()?.Content?["name"].ToString()
						?? mBridgeChannelContent?["displayname"]?.ToString()
						?? jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.room.member" && e.Content != null && e.Content.TryGetValue("displayname", out object value) && value?.ToString() != uzivatelskeJmeno && value?.ToString().Contains("bridge bot") == false)?.LastOrDefault()?.Content["displayname"].ToString()
						?? null;*/ //"ID " + jedenChatMatrix.Key;

                    //string nazevChatu =
                    //	// 1) Room name event
                    //	jedenChatMatrix.Value.Timeline?.Events?
                    //	.Where(
                    //		e => e.Type == "m.room.name"
                    //		&& e.Content != null)
                    //	.LastOrDefault()?.Content["name"].ToString()

                    //	// 2) Bridge displayname
                    //	?? mBridgeChannelContent?["displayname"]?.ToString()

                    //	// 3) Member displayname (not the user, not a bot)
                    //	?? jedenChatMatrix.Value.State?.Events?
                    //	.Where(
                    //		e => e.Type == "m.room.member"
                    //		&& e.Content != null && e.Content.TryGetValue("displayname", out var dnObj)
                    //		&& dnObj?.ToString() != uzivatelskeJmeno
                    //		&& dnObj?.ToString()?.Contains("bridge bot") == false)
                    //	.LastOrDefault()?.Content["displayname"].ToString()

                    //	// 4) Fallback
                    //	?? null;

                    List<ClenChatu> clenoveChatu = new List<ClenChatu>();


                    string nazevChatu =
                        // 1) Room name event
                        jedenChatMatrix.Value.Timeline?.Events?
                            .Where(e =>
                                e.Type == "m.room.name"
                                && ZiskatHodnotuDictionary(e.Content, "name") != null)
                            .Select(e => ZiskatHodnotuDictionary(e.Content, "name"))
                            .LastOrDefault()

                        // 2) Bridge displayname
                        ?? mBridgeChannelContent?["displayname"]?.ToString()

                        ?? null;

                    if (nazevChatu == null)
                    {
                        foreach (var jedenClenChatu in jedenChatMatrix.Value.State?.Events?
                                .Where(e =>
                                    e.Type == "m.room.member"
                                    && ZiskatHodnotuDictionary(e.Content, "displayname") != null
                                    && ZiskatHodnotuDictionary(e.Content, "displayname") != uzivatelskeJmeno
                                    /*&& ZiskatHodnotuDictionary(e.Content, "displayname")?.Contains("bridge bot") == false*/))
                        {
                            clenoveChatu.Add(new ClenChatu { ZobrazovaneJmeno = ZiskatHodnotuDictionary(jedenClenChatu.Content, "displayname"), ProfilovaFotka = ZiskatHodnotuDictionary(jedenClenChatu.Content, "avatar_url"), MatrixIdUzivatele = jedenClenChatu.Sender });
                        }

                        foreach (var jedenClenChatu in jedenChatMatrix.Value.Timeline?.Events?
                                .Where(e =>
                                    e.Type == "m.room.member"
                                    && ZiskatHodnotuDictionary(e.Content, "displayname") != null
                                    && ZiskatHodnotuDictionary(e.Content, "displayname") != uzivatelskeJmeno
                                    /*&& ZiskatHodnotuDictionary(e.Content, "displayname")?.Contains("bridge bot") == false*/))
                        {
                            if (!clenoveChatu.Any(x => x.ZobrazovaneJmeno == ZiskatHodnotuDictionary(jedenClenChatu.Content, "displayname")))
                            {
                                clenoveChatu.Add(new ClenChatu { ZobrazovaneJmeno = ZiskatHodnotuDictionary(jedenClenChatu.Content, "displayname"), ProfilovaFotka = ZiskatHodnotuDictionary(jedenClenChatu.Content, "avatar_url"), MatrixIdUzivatele = jedenClenChatu.Sender });
                            }
                        }

                        if (clenoveChatu.Count == 1)
                        {
                            nazevChatu = clenoveChatu[0].ZobrazovaneJmeno;
                            if (urlObrazkuChatu == null)
                            {
                                urlObrazkuChatu = clenoveChatu[0].ProfilovaFotka;
                            }
                        }
                        else if (clenoveChatu.Count == 0)
                        {
                            nazevChatu = "Prázdný chat";
                        }
                        else if (clenoveChatu.Count > 1) // 2 a víc členů – nejdřív odstraníme Bridge bota
                        {
                            clenoveChatu.RemoveAll(x =>
                x.ZobrazovaneJmeno.IndexOf("bridge bot", StringComparison.OrdinalIgnoreCase) >= 0);

                            nazevChatu = clenoveChatu.LastOrDefault().ZobrazovaneJmeno;
                            if (urlObrazkuChatu == null)
                            {
                                urlObrazkuChatu = clenoveChatu.LastOrDefault().ProfilovaFotka;
                            }
                        }
                    }

                    /*

                    ?? jedenChatMatrix.Value.State?.Events?
                        .Where(e =>
                            e.Type == "m.room.member"
                            && ZiskatHodnotuDictionary(e.Content, "displayname") != null
                            && ZiskatHodnotuDictionary(e.Content, "displayname") != uzivatelskeJmeno
                            && ZiskatHodnotuDictionary(e.Content, "displayname")?.Contains("bridge bot") == false)
                        .Select(e => ZiskatHodnotuDictionary(e.Content, "displayname"))
                        .LastOrDefault()

                    ?? jedenChatMatrix.Value.Timeline?.Events?
                        .Where(e =>
                            e.Type == "m.room.member"
                            && ZiskatHodnotuDictionary(e.Content, "displayname") != null
                            && ZiskatHodnotuDictionary(e.Content, "displayname") != uzivatelskeJmeno
                            && ZiskatHodnotuDictionary(e.Content, "displayname")?.Contains("bridge bot") == false)
                        .Select(e => ZiskatHodnotuDictionary(e.Content, "displayname"))
                        .LastOrDefault()

                    ?? null;*/

                    

                    Event posledniZprava = jedenChatMatrix.Value.Timeline?.Events?
                                .Where(e => e.Type == "m.room.message")
                                .LastOrDefault(v => v != null);

                    long unixoveSekundyPosledniZpravy = posledniZprava?.OriginServerTs ?? 0;

                    //string zobrazovaneJmenoOdesilatelePosledniZpravy = clenoveChatu?.FirstOrDefault(e => e.MatrixIdUzivatele == posledniZprava?.Sender)?.ZobrazovaneJmeno ?? posledniZprava?.Sender ?? "Prázdná konverzace";

                    string posledniZpravaText = /*zobrazovaneJmenoOdesilatelePosledniZpravy + " | " +*/ ZiskatHodnotuDictionary(posledniZprava?.Content, "body")?.Replace("\r", " ")?.Replace("\n", " ");

                    /*string posledniZprava = jedenChatMatrix.Value.Timeline?.Events?
                                .Where(e => e.Type == "m.room.message")
                                .Select(e => ZiskatHodnotuDictionary(e.Content, "body"))
                                .LastOrDefault(v => v != null)
                            ?? jedenChatMatrix.Value.Timeline?.Events?.LastOrDefault()?.Type;*/

                    string zobrazovanyCasPosledniZpravy = "";
                    DateTime dateTimePosledniZpravy = DateTimeOffset.FromUnixTimeMilliseconds(unixoveSekundyPosledniZpravy).LocalDateTime;

                    if (dateTimePosledniZpravy.Day == DateTime.Now.Day && dateTimePosledniZpravy.Month == DateTime.Now.Month && dateTimePosledniZpravy.Year == DateTime.Now.Year) // Je to dneska, dát jenom čas
                    {
                        zobrazovanyCasPosledniZpravy = dateTimePosledniZpravy.ToString("HH:mm");
                    }
                    else if (dateTimePosledniZpravy.Day == DateTime.Now.AddDays(-1).Day && dateTimePosledniZpravy.Month == DateTime.Now.AddDays(-1).Month && dateTimePosledniZpravy.Year == DateTime.Now.AddDays(-1).Year)
                    {
                        zobrazovanyCasPosledniZpravy = "včera";
                    }
                    else
                    {
                        zobrazovanyCasPosledniZpravy = dateTimePosledniZpravy.ToString("d. M. yyyy");
                    }

                    MatrixSeznamChatu.Add(new MatrixSeznamChatu_JedenChat
                    {
                        IdChatu = jedenChatMatrix.Key,
                        NazevChatu = nazevChatu,
                        PosledniZprava = posledniZpravaText,
                        UnixoveSekundyPosledniZpravy = unixoveSekundyPosledniZpravy,
                        ZobrazovanyCasPosledniZpravy = zobrazovanyCasPosledniZpravy,
                        ObrazekChatu = await NacistMatrixObrazek(urlObrazkuChatu),
                        //Timeline = jedenChatMatrix.Value.Timeline,
                        ClenoveChatu = clenoveChatu // Bez aktuálního uživatele
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
                    Title = "Chyba při načítání nebo zpracovávání dat",
                    CloseButtonText = "Zavřít"
                }.ShowAsync();

                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPage.PageHeader.Text = "Všechny konverzace";

        }

        private void ListViewChaty_ItemClick(object sender, ItemClickEventArgs e)
        {
            MatrixSeznamChatu_JedenChat kliknutyChat = (MatrixSeznamChatu_JedenChat)e.ClickedItem;

            MainPage.NavigovatNaStranku(typeof(StrankaJedenChat), kliknutyChat.IdChatu);
        }
    }
}
