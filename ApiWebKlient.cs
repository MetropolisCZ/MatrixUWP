using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static MatrixUWP.MainPage;

namespace MatrixUWP
{
    public class ApiWebKlient
    {

        public static HttpClient httpClient = new HttpClient();
        public static BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        public static BackgroundUploader backgroundUploader = new BackgroundUploader() { Method = "PUT" };

        public static StorageFolder DocasnaSlozka { get; set; } = ApplicationData.Current.TemporaryFolder;


        public enum TypyHTTPrequestu
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }

        public static async Task<string> NacistStrankuRestApi(string UrlkZiskani, TypyHTTPrequestu typHTTPrequestu = TypyHTTPrequestu.Get, string teloHTTPrequestu = null)
        {
            bool prvniPokus = true;
        druhyPokus:

            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (typHTTPrequestu == TypyHTTPrequestu.Get)
            {
                httpResponse = await httpClient.GetAsync(new Uri(UrlkZiskani));
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Patch)
            { // Upraví vlastnosti, zachová soubor

                httpResponse = await httpClient.SendRequestAsync(new HttpRequestMessage(HttpMethod.Patch, new Uri(UrlkZiskani)) { Content = new HttpStringContent(teloHTTPrequestu, UnicodeEncoding.Utf8, "application/json") });
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Post)
            { // Posílá data na server, narozdíl od PUT je možné POST volat vícekrát což může mít za následek například vícenásobné vytvoření téže položky

                httpResponse = await httpClient.PostAsync(new Uri(UrlkZiskani), new HttpStringContent(teloHTTPrequestu, UnicodeEncoding.Utf8, "application/json"));

            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Put)
            {

            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Delete)
            {
                httpResponse = await httpClient.DeleteAsync(new Uri(UrlkZiskani));
            }

            if (httpResponse.IsSuccessStatusCode || (typHTTPrequestu == TypyHTTPrequestu.Delete && httpResponse.StatusCode == HttpStatusCode.NoContent))
            {

                return await httpResponse.Content.ReadAsStringAsync();

            }
            else
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && prvniPokus)
                {

                    bool zobrazitPrihlaseniAutomaticky = true;
                    NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);

                    throw new OperationCanceledException();
                }
                else
                {
                    ContentDialog dialogHTTPchyba = new ContentDialog()
                    {
                        Title = "HTTP odpověď " + httpResponse.StatusCode,
                        Content = await httpResponse.Content.ReadAsStringAsync() + "\n\n" + UrlkZiskani,
                        CloseButtonText = "Zavřit"
                    };

                    _ = await dialogHTTPchyba.ShowAsync();
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
                    }
                    else
                    {
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    }

                    throw new System.Net.Http.HttpRequestException();
                }

            }

        }

        public static async Task<BitmapImage> NacistMatrixObrazek(string urlObrazku)
        {
            if (urlObrazku == null)
            {
                return null;
            }

            BitmapImage matrixObrazek = new BitmapImage();

            IBuffer BufferObrazku = await NacistBufferRestApi("https://" + StrankaChaty.matrixServer + "/_matrix/client/v1/media/download/" + urlObrazku.Remove(0, 6));

            var memoryStream = new InMemoryRandomAccessStream();
            await memoryStream.WriteAsync(BufferObrazku);
            memoryStream.Seek(0);

            try
            {
                await matrixObrazek.SetSourceAsync(memoryStream);
                return matrixObrazek;
            }
            catch
            {
                byte[] bytes = BufferObrazku.ToArray();
                bool jeToHeic =
                    bytes.Length > 12 &&
                    bytes[4] == 0x66 && // 'f'
                    bytes[5] == 0x74 && // 't'
                    bytes[6] == 0x79 && // 'y'
                    bytes[7] == 0x70;   // 'p'
                                        // Výchozí iPhone formát, není podporovaný na W10M

                if (jeToHeic)
                {
                    Debug.WriteLine("Nepodporovaný formát obrázku");
                }
                else
                {
                    Debug.WriteLine("Neznámá chyba při načítání obrázku");
                }

                return null;
            }
        }



        public static async Task<BitmapImage> NacistMatrixObrazekDoDocasneSlozky(string urlObrazku, string nazevStazenehoObrazku)
        {

            if (urlObrazku == null || nazevStazenehoObrazku == null)
            {
                return null;
            }

            BitmapImage matrixObrazek = new BitmapImage();

            IBuffer BufferObrazku;
            StorageFile ulozenyAktualniObrazekFile;

            // Stáhnout

            Debug.WriteLine("Stahování obrázku ze serveru");


            BufferObrazku = await NacistBufferRestApi("https://" + StrankaChaty.matrixServer + "/_matrix/client/v1/media/download/" + urlObrazku.Remove(0, 6));


            var memoryStream = new InMemoryRandomAccessStream();
            await memoryStream.WriteAsync(BufferObrazku);
            memoryStream.Seek(0);

            try
            {
                await matrixObrazek.SetSourceAsync(memoryStream);
            }
            catch
            {
                byte[] bytes = (BufferObrazku).ToArray();
                bool jeToHeic =
                    bytes.Length > 12 &&
                    bytes[4] == 0x66 && // 'f'
                    bytes[5] == 0x74 && // 't'
                    bytes[6] == 0x79 && // 'y'
                    bytes[7] == 0x70;   // 'p'
                                        // Výchozí iPhone formát, není podporovaný na W10M

                if (jeToHeic)
                {
                    Debug.WriteLine("Nepodporovaný formát obrázku");
                }
                else
                {
                    Debug.WriteLine("Neznámá chyba při načítání obrázku");
                }

                return null;
            }

            ulozenyAktualniObrazekFile = await DocasnaSlozka.CreateFileAsync(nazevStazenehoObrazku, CreationCollisionOption.GenerateUniqueName);

            await FileIO.WriteBufferAsync(ulozenyAktualniObrazekFile, BufferObrazku);

            return matrixObrazek;



            // Načíst
            /*IRandomAccessStream fileStream = await ulozenyAktualniObrazekFile.OpenAsync(FileAccessMode.Read);

            await matrixObrazek.SetSourceAsync(fileStream);

            zpravasObrazkem.ObrazekZpravy = matrixObrazek;
            zpravasObrazkem.NazevObrazkuZpravy = nazevAktualnihoObrazku;*/

        }

        public async static Task<BitmapImage> ObrazekNacistzCacheNeboStahnout(string urlObrazku, string nazevSouboruObrazkuChatu = null, string koncovkaSouboruObrazkuChatu = null)
        {
            try
            {
                // Při parsování strankachaty získat koncovku souboru a uložit ji

                if (nazevSouboruObrazkuChatu == null || koncovkaSouboruObrazkuChatu == null)
                { // Není uložen název, stáhnout název

                    HttpResponseMessage httpResponse = new HttpResponseMessage();
                    httpResponse = await httpClient.GetAsync(new Uri("https://" + StrankaChaty.matrixServer + "/_matrix/client/v1/media/download/" + urlObrazku.Remove(0, 6)));
                    string koncovkaObrazku = httpResponse.Content.Headers.ContentType.ToString().Split('/')[1];

                    string novyNazevSouboru = urlObrazku.Split('/').Last() + "." + koncovkaObrazku;

                    IStorageItem ulozenyAktualniObrazek = await DocasnaSlozka.TryGetItemAsync(novyNazevSouboru);

                    if (ulozenyAktualniObrazek != null)
                    { // Obrázek je v cache, načíst obrázek

                        //Debug.WriteLine("ObrazekNacistzCacheNeboStahnout(): Načítání obrázku z cache");

                        StorageFile ulozenyAktualniObrazekFile = (StorageFile)ulozenyAktualniObrazek;
                        using (IRandomAccessStream fileStream = await ulozenyAktualniObrazekFile.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(fileStream);
                            return bitmapImage;
                        }
                    }
                    else
                    { // Obrázek není v cache, stáhnout obrázek

                        Debug.WriteLine("ObrazekNacistzCacheNeboStahnout(): Stahování obrázku ze serveru (" + urlObrazku + ")");

                        return await NacistMatrixObrazekDoDocasneSlozky(urlObrazku, novyNazevSouboru);
                    }

                }
                else
                {
                    // TODO!
                    return null;
                }
            }
            catch
            {
                return await NacistMatrixObrazekDoDocasneSlozky(urlObrazku, nazevSouboruObrazkuChatu + "." + koncovkaSouboruObrazkuChatu);
            }
        }


        public static async Task<IBuffer> NacistBufferRestApi(string UrlkZiskani, TypyHTTPrequestu typHTTPrequestu = TypyHTTPrequestu.Get, string teloHTTPrequestu = null)
        {
            bool prvniPokus = true;
        druhyPokus:

            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (typHTTPrequestu == TypyHTTPrequestu.Get)
            {
                httpResponse = await httpClient.GetAsync(new Uri(UrlkZiskani));
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Patch)
            { // Upraví vlastnosti, zachová soubor

                httpResponse = await httpClient.SendRequestAsync(new HttpRequestMessage(HttpMethod.Patch, new Uri(UrlkZiskani)) { Content = new HttpStringContent(teloHTTPrequestu, UnicodeEncoding.Utf8, "application/json") });
            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Post)
            { // Posílá data na server, narozdíl od PUT je možné POST volat vícekrát což může mít za následek například vícenásobné vytvoření téže položky

                httpResponse = await httpClient.PostAsync(new Uri(UrlkZiskani), new HttpStringContent(teloHTTPrequestu, UnicodeEncoding.Utf8, "application/json"));

            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Put)
            {

            }
            else if (typHTTPrequestu == TypyHTTPrequestu.Delete)
            {
                httpResponse = await httpClient.DeleteAsync(new Uri(UrlkZiskani));
            }

            if (httpResponse.IsSuccessStatusCode || (typHTTPrequestu == TypyHTTPrequestu.Delete && httpResponse.StatusCode == HttpStatusCode.NoContent))
            {

                return await httpResponse.Content.ReadAsBufferAsync();

            }
            else
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && prvniPokus)
                {

                    bool zobrazitPrihlaseniAutomaticky = true;
                    NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);

                    throw new OperationCanceledException();
                }
                else
                {
                    ContentDialog dialogHTTPchyba = new ContentDialog()
                    {
                        Title = "HTTP odpověď " + httpResponse.StatusCode,
                        Content = await httpResponse.Content.ReadAsStringAsync() + "\n\n" + UrlkZiskani,
                        CloseButtonText = "Zavřit"
                    };

                    _ = await dialogHTTPchyba.ShowAsync();
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool zobrazitPrihlaseniAutomaticky = true;
                        NavigovatNaStranku(typeof(StrankaNastaveni), zobrazitPrihlaseniAutomaticky);
                    }
                    else
                    {
                        MainPage.NavigovatNaStranku(typeof(StrankaNastaveni));
                    }

                    throw new System.Net.Http.HttpRequestException();
                }

            }

        }



        public static string ZiskatHodnotuDictionary(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
                return null;

            return dictionary.TryGetValue(key, out object obj)
                ? obj?.ToString()
                : null;

        }



        public static IDictionary<string, object> ZiskatHodnotuDictionaryVratitDictionary(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null) return null;
            if (!dictionary.TryGetValue(key, out object obj)) return null;

            if (obj is JObject jObj)
            {
                return jObj.ToObject<Dictionary<string, object>>();
            }

            return obj as IDictionary<string, object>;

        }




    }










    // ✅ Reactive model for a single download
    public class DownloadItem : INotifyPropertyChanged
    {
        private string fileName;
        private double progress;
        private string status;

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public StorageFile StorageFile { get; set; }
        public bool JenomTemp { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ✅ Singleton manager that holds all downloads
    public class DownloadManager
    {
        private static DownloadManager _instance;

        public static DownloadManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DownloadManager();
                return _instance;
            }
        }

        public ObservableCollection<DownloadItem> Downloads { get; } = new ObservableCollection<DownloadItem>();
        public ObservableCollection<DownloadItem> Uploads { get; } = new ObservableCollection<DownloadItem>();
        private DownloadManager() { }
    }




}
