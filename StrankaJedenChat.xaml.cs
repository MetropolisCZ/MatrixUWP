using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
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

        public static MatrixSeznamChatu_JedenChat chatKterySeMaZobrazit = new MatrixSeznamChatu_JedenChat();
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
                        /*StorageFile StazenyObrazek = await DocasnaSlozka.CreateFileAsync(JednaZpravaAktualniKonverzace.EventId + "." + ZiskatHodnotuDictionary(ZiskatHodnotuDictionaryVratitDictionary(JednaZpravaAktualniKonverzace.Content, "info"), "mimetype").Split('/')[1], CreationCollisionOption.GenerateUniqueName);

                        await FileIO.WriteBufferAsync(StazenyObrazek, await NacistBufferRestApi("https://" + StrankaChaty.matrixServer + "/_matrix/client/v1/media/download/" + ZiskatHodnotuDictionary(JednaZpravaAktualniKonverzace.Content, "url").Remove(0, 6)));

                        await Windows.System.Launcher.LaunchFileAsync(StazenyObrazek);*/

                        string nazevAktualnihoObrazku = JednaZpravaAktualniKonverzace.EventId + "." + ZiskatHodnotuDictionary(ZiskatHodnotuDictionaryVratitDictionary(JednaZpravaAktualniKonverzace.Content, "info"), "mimetype").Split('/')[1];

                        IStorageItem ulozenyAktualniObrazek = await DocasnaSlozka.TryGetItemAsync(nazevAktualnihoObrazku);

                        if (ulozenyAktualniObrazek != null)
                        {
                            StorageFile ulozenyAktualniObrazekFile = (StorageFile)ulozenyAktualniObrazek;
                            using (IRandomAccessStream fileStream = await ulozenyAktualniObrazekFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(fileStream);

                                JednaZpravaAktualniKonverzace.ObrazekZpravy = bitmapImage;
                                JednaZpravaAktualniKonverzace.NazevObrazkuZpravy = nazevAktualnihoObrazku;
                            }
                        }



                        //.EventId

                        //JednaZpravaAktualniKonverzace.ObrazekZpravy = await NacistMatrixObrazek(ZiskatHodnotuDictionary(JednaZpravaAktualniKonverzace.Content, "url")) ?? null;

                        //bool souborOtevrenvAplikaciKalendar = await Windows.System.Launcher.LaunchFileAsync(DopravniSpojeniKalendarIcs, new Windows.System.LauncherOptions() { TargetApplicationPackageFamilyName = "64885BlueEdge.OneCalendar_8kea50m9krsh2" });
                        //// Get-AppxPackage | Select - Object Name, PackageFamilyName
                        //// Takhle se získaj všechny jména aplikací + názvy rodin balíčků (PFN)

                        //if (souborOtevrenvAplikaciKalendar)
                        //{
                        //    Debug.WriteLine("Otevřeno v kalendáři");
                        //}
                        //else
                        //{
                        //    Debug.Fail("Otevření v kalendáři selhalo");
                        //}
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

        private async void ObrazekVeZprave_Kliknuto(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FrameworkElement kliknutySoubor = (FrameworkElement)sender;
            Event kliknutySouborDataKontext = (Event)kliknutySoubor.DataContext;

            try
            {
                _ = await Windows.System.Launcher.LaunchFileAsync(await DocasnaSlozka.GetFileAsync(kliknutySouborDataKontext.NazevObrazkuZpravy));
            }
            catch
            {

            }
        }

        private async void StahnoutObrazek_Click(object sender, RoutedEventArgs e)
        {

            Button kliknutySoubor = (Button)sender;
            Event kliknutySouborDataKontext = (Event)kliknutySoubor.DataContext;

            string nazevAktualnihoObrazku = kliknutySouborDataKontext.EventId + "." + ZiskatHodnotuDictionary(ZiskatHodnotuDictionaryVratitDictionary(kliknutySouborDataKontext.Content, "info"), "mimetype").Split('/')[1];


            BitmapImage praveStazenyObrazek = await NacistMatrixObrazekDoDocasneSlozky(ZiskatHodnotuDictionary(kliknutySouborDataKontext.Content, "url"), nazevAktualnihoObrazku);

            if (praveStazenyObrazek != null)
            {
                kliknutySouborDataKontext.ObrazekZpravy = praveStazenyObrazek;
                kliknutySouborDataKontext.NazevObrazkuZpravy = nazevAktualnihoObrazku;
                ListViewZpravyChaty.Focus(FocusState.Programmatic);
            }

            

            /*

            try
            {
                string nazevAktualnihoObrazku = kliknutySouborDataKontext.EventId + "." + ZiskatHodnotuDictionary(ZiskatHodnotuDictionaryVratitDictionary(kliknutySouborDataKontext.Content, "info"), "mimetype").Split('/')[1];
                
                if (await DocasnaSlozka.TryGetItemAsync(nazevAktualnihoObrazku) == null)
                {
                    StorageFile StazenyObrazek = await DocasnaSlozka.CreateFileAsync(nazevAktualnihoObrazku, CreationCollisionOption.GenerateUniqueName);

                    await FileIO.WriteBufferAsync(StazenyObrazek, await NacistBufferRestApi("https://" + StrankaChaty.matrixServer + "/_matrix/client/v1/media/download/" + ZiskatHodnotuDictionary(kliknutySouborDataKontext.Content, "url").Remove(0, 6)));

                    using (IRandomAccessStream fileStream = await StazenyObrazek.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(fileStream);

                        kliknutySouborDataKontext.ObrazekZpravy = bitmapImage;
                        kliknutySouborDataKontext.NazevObrazkuZpravy = nazevAktualnihoObrazku;
                    }
                }
            }
            catch
            {

            }*/

        }
    }
}
