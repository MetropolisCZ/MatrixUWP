using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using static MatrixUWP.ApiWebKlient;
using Windows.Networking.BackgroundTransfer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

// Dokumentaci k šabloně Prázdná aplikace najdete na adrese https://go.microsoft.com/fwlink/?LinkId=234238

namespace MatrixUWP
{
    /// <summary>
    /// Prázdná stránka, která se dá použít samostatně nebo se na ni dá přejít v rámci
    /// </summary>
    public sealed partial class StrankaNastaveni : Page
    {

        public StrankaNastaveni()
        {
            InitializeComponent();

        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //AccountsSettingsPane.Show();

            StackPanel contentDialogPrihlaseni_stackPanel = new StackPanel();
            TextBox contentDialogPrihlaseni_textBox_server = new TextBox()
            {
                PlaceholderText = "Server"
            };
            TextBox contentDialogPrihlaseni_textBox_uzivatelskeJmeno = new TextBox()
            {
                PlaceholderText = "Uživatelské jméno"
            };
            TextBox contentDialogPrihlaseni_textBox_heslo = new TextBox()
            {
                PlaceholderText = "Heslo"
            };

            contentDialogPrihlaseni_stackPanel.Children.Add(contentDialogPrihlaseni_textBox_server);
            contentDialogPrihlaseni_stackPanel.Children.Add(contentDialogPrihlaseni_textBox_uzivatelskeJmeno);
            contentDialogPrihlaseni_stackPanel.Children.Add(contentDialogPrihlaseni_textBox_heslo);

            ContentDialog contentDialogPrihlaseni = new ContentDialog()
            {
                Title = "Přihlášení",
                PrimaryButtonText = "Přihlásit se",
                CloseButtonText = "Zrušit",
                Content = contentDialogPrihlaseni_stackPanel
            };

            contentDialogPrihlaseni_textBox_server.Focus(FocusState.Programmatic);
            ContentDialogResult contentDialogResult = await contentDialogPrihlaseni.ShowAsync();

            //string clientId = "d0342bc7-f4d3-422e-97d7-354ecdc21ae7"; // Obtain your clientId from the Azure Portal
            //WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "Files.ReadWrite.All", clientId);
            //request.Properties.Add("resource", "https://graph.microsoft.com");
            //WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);


            if (contentDialogResult == ContentDialogResult.Primary && contentDialogPrihlaseni_textBox_server.Text.Length > 0 && contentDialogPrihlaseni_textBox_uzivatelskeJmeno.Text.Length > 0 && contentDialogPrihlaseni_textBox_heslo.Text.Length > 0)
            {

                httpClient.DefaultRequestHeaders.Clear();
                ApplicationData.Current.LocalSettings.Values.Clear();

                string UrlkZiskani = "https://" + contentDialogPrihlaseni_textBox_server.Text + "/_matrix/client/r0/login";
                //string teloHTTPrequestu = "{ type': 'm.login.password', 'identifier': { 'type': 'm.id.user', 'user': '" + contentDialogPrihlaseni_textBox_uzivatelskeJmeno.Text + "' }, 'password': '" + contentDialogPrihlaseni_textBox_heslo.Text + "' } ";
                var teloHTTPrequestu = new
                {
                    type = "m.login.password",
                    identifier = new
                    {
                        type = "m.id.user",
                        user = contentDialogPrihlaseni_textBox_uzivatelskeJmeno.Text
                    },
                    password = contentDialogPrihlaseni_textBox_heslo.Text
                };


                string pristupovyToken = "";
                try
                {
                    pristupovyToken = JObject.Parse(await NacistStrankuRestApi(UrlkZiskani, TypyHTTPrequestu.Post, JsonConvert.SerializeObject(teloHTTPrequestu))).SelectToken("access_token").ToString();

                }
                catch
                {
                    return;
                }

                var headers = httpClient.DefaultRequestHeaders;
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", pristupovyToken);
                ApplicationData.Current.LocalSettings.Values["pristupovyToken"] = pristupovyToken;
                ApplicationData.Current.LocalSettings.Values["MatrixServer"] = contentDialogPrihlaseni_textBox_server.Text;
                ApplicationData.Current.LocalSettings.Values["uzivatelskeJmeno"] = contentDialogPrihlaseni_textBox_uzivatelskeJmeno.Text;
                MainPage.NavigovatNaStranku(typeof(StrankaChaty));


                //MainPage.NavigovatNaStranku(typeof(StrankaSoubory));

                //JObject repository_url = JObject.Parse(await NacistStrankuRestApi("https://graph.microsoft.com/v1.0/me"));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += NaplnitPrihlasovaciMoznosti;
            MainPage.PageHeader.Text = "Nastavení";

            if (e?.Parameter != null && (bool)e.Parameter) // zobrazitPrihlaseniAutomaticky
            {
                //AccountsSettingsPane.Show();
            }
        }


    }
}
