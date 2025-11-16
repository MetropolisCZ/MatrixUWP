using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
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
