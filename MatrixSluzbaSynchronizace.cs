using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using static MatrixUWP.MatrixDatabazeObjekty;
using static MatrixUWP.ApiWebKlient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace MatrixUWP
{
    public sealed class MatrixSluzbaSynchronizace
    {
        public ObservableCollection<MatrixDatabaze_Mistnost> SeznamChatu = new ObservableCollection<MatrixDatabaze_Mistnost>();
        public ObservableCollection<MatrixDatabaze_Udalost> VybranaKonzervace_Udalosti = new ObservableCollection<MatrixDatabaze_Udalost>();
        public ObservableCollection<MatrixDatabaze_Stav> VybranaKonverzace_Stavy = new ObservableCollection<MatrixDatabaze_Stav>();
        public bool synchonizacniSmyckaSpustena = false;

        public string uzivatelskeJmeno = ApplicationData.Current.LocalSettings.Values["uzivatelskeJmeno"]?.ToString();
        public string matrixServer = ApplicationData.Current.LocalSettings.Values["MatrixServer"]?.ToString();
        public string pristupovyToken = ApplicationData.Current.LocalSettings.Values["pristupovyToken"]?.ToString();
        public string tokenProOfsetSynchronizace = ApplicationData.Current.LocalSettings.Values["tokenProOfsetSynchronizace"]?.ToString();


        // Jediná instance třídy je tohle: _instance
        // Když někdo řekne o MatrixSluzbaSynchronizace.Instance, buď se vytvoří, nebo se znova použije – tj. nikdy nebudou dvě instance
        private static MatrixSluzbaSynchronizace _instance;

        public static MatrixSluzbaSynchronizace Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MatrixSluzbaSynchronizace();

                return _instance;
            }
        }

        // Konstruktor je privátní – nikdo nemůže volat new MatrixDatabaze()
        private MatrixSluzbaSynchronizace()
        {
            var headers = httpClient.DefaultRequestHeaders;
            if (pristupovyToken != null)
            {
                headers.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", pristupovyToken);
            }
        }


        public async Task StahnoutCelySynchronizacniSoubor()
        {
            try
            {
                // Úložiště DB – C:\Users\tomas\AppData\Local\Packages\FirmaMetropolis.MetropolisMatrixklient_fm85nwa52bhpm\LocalState

                string UrlSyncFiltrovana = "https://" + matrixServer + "/_matrix/client/r0/sync"; // ?filter={\"room\":{\"timeline\":{\"limit\":2},\"state\":{\"lazy_load_members\":true}}}
                var aaa = await NacistStrankuRestApi(UrlSyncFiltrovana);

                MatrixDatabaze.Instance.OdstranitObsahVsechTabulek();

                MatrixSyncOdpoved matrixSyncOdpoved = JsonConvert.DeserializeObject<MatrixSyncOdpoved>(aaa);

                foreach (var jedenChatMatrix in matrixSyncOdpoved.Rooms.Join)
                {
                    JObject mBridgeChannelContent = (JObject)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.bridge" && e.Content != null)?.LastOrDefault()?.Content?["channel"]);

                    string urlObrazkuChatu = (string)(jedenChatMatrix.Value.State?.Events?.Where(e => e.Type == "m.room.avatar" && e.Content != null)?.LastOrDefault()?.Content?["url"]) ?? null;


                    List<ClenChatu> clenoveChatu = new List<ClenChatu>();


                    string nazevChatu =
                        // 1) Room name event
                        jedenChatMatrix.Value.Timeline?.Events?
                            .Where(e =>
                                e.Type == "m.room.name"
                                && e.ContentJson.GetValue("name").ToString() != null)
                            .Select(e => e.ContentJson.GetValue("name").ToString())
                            .LastOrDefault()

                        // 2) Bridge displayname
                        ?? mBridgeChannelContent?["displayname"]?.ToString()

                        ?? null;

                    //if (nazevChatu == null)
                    //{

                    // Chtěl bych zkusit načítat všechny uživatele vždycky – kvůli použití potom v konverzaci. Může to být zásadní zpomalení, lepší by bylo to načítat až potom, když uživatel vybere tu jednotlivou konverzaci
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
                                && e.ContentJson.GetValue("displayname")?.ToString() != null
                                && e.ContentJson.GetValue("displayname")?.ToString() != uzivatelskeJmeno
                                /*&& ZiskatHodnotuDictionary(e.Content, "displayname")?.Contains("bridge bot") == false*/))
                    {
                        if (!clenoveChatu.Any(x => x.ZobrazovaneJmeno == jedenClenChatu.ContentJson.GetValue("displayname").ToString()))
                        {
                            clenoveChatu.Add(new ClenChatu { ZobrazovaneJmeno = jedenClenChatu.ContentJson.GetValue("displayname")?.ToString(), ProfilovaFotka = jedenClenChatu.ContentJson.GetValue("avatar_url")?.ToString(), MatrixIdUzivatele = jedenClenChatu.Sender });
                        }
                    }

                    if (nazevChatu == null)
                    {

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
                        else
                        { // 2 nebo víc členů 
                            clenoveChatu.RemoveAll(x => x.ZobrazovaneJmeno.IndexOf("bridge bot", StringComparison.OrdinalIgnoreCase) >= 0);

                            if (clenoveChatu.Count == 1)
                            {
                                nazevChatu = clenoveChatu.LastOrDefault().ZobrazovaneJmeno;
                            }
                            else
                            {
                                nazevChatu = "Skupina (" + clenoveChatu[0].ZobrazovaneJmeno + ", " + clenoveChatu[1].ZobrazovaneJmeno + ", …)";
                            }


                            if (urlObrazkuChatu == null)
                            {
                                urlObrazkuChatu = clenoveChatu.LastOrDefault().ProfilovaFotka;
                            }
                        }
                    }


                    if (urlObrazkuChatu == null)
                    {
                        urlObrazkuChatu = "";
                    }


                    // -------------------------------------------------
                    // Události místnosti
                    // -------------------------------------------------

                    long posledniCasoveRazitkoMistnosti = 0;
                    string textPosledniZpravyNahled = "Chyba při zpracovávání obsahu";

                    foreach (Event jedenChatMatrix_udalost in jedenChatMatrix.Value.Timeline.Events)
                    {
                        MatrixDatabaze.Instance.VlozitUdalostDoDatabaze(new MatrixDatabaze_Udalost
                        {
                            CasoveRazitko = jedenChatMatrix_udalost.OriginServerTs,
                            Druh = jedenChatMatrix_udalost.Type,
                            IdMistnosti = jedenChatMatrix.Key,
                            IdUdalosti = jedenChatMatrix_udalost.EventId,
                            Odesilatel = jedenChatMatrix_udalost.Sender,
                            ObsahJSON = jedenChatMatrix_udalost.ContentJson.ToString()
                        });

                        if (jedenChatMatrix_udalost.Type == "m.room.message")
                        { // Když je událost typu zpráva, tak použijeme její časové razítko pro řazení
                            posledniCasoveRazitkoMistnosti = jedenChatMatrix_udalost.OriginServerTs;
                            string textPosledniZpravyNahledMozna = jedenChatMatrix_udalost.ContentJson.GetValue("body")?.ToString();
                            if (textPosledniZpravyNahledMozna != null)
                            {
                                textPosledniZpravyNahled = textPosledniZpravyNahledMozna;
                            }
                        }
                    }

                    // -------------------------------------------------
                    // Obrázek místnosti
                    // -------------------------------------------------



                    MatrixDatabaze.Instance.VlozitMistnostDoDatabaze(new MatrixDatabaze_Mistnost
                    {
                        IdMistnosti = jedenChatMatrix.Key,
                        Nazev = nazevChatu,
                        UrlObrazku = urlObrazkuChatu,
                        PocetNeprectenych = jedenChatMatrix.Value.Unread_notifications.Notification_count,
                        CasovaZnamkaPosledniUdalosti = posledniCasoveRazitkoMistnosti,
                        TextPosledniZpravyNahled = textPosledniZpravyNahled
                    });



                }


                // Teď uložíme token pro offset synchronizace (next_batch)
                ApplicationData.Current.LocalSettings.Values["tokenProOfsetSynchronizace"] = matrixSyncOdpoved.NextBatch;
                tokenProOfsetSynchronizace = matrixSyncOdpoved.NextBatch;

                SeznamChatu = await MatrixDatabaze.Instance.VybratVsechnyMistnostizDatabaze();

#pragma warning disable CS4014 // Protože se toto volání neočekává, vykonávání aktuální metody pokračuje před dokončením volání.
                SpustitSynchronizacniSmycku();
#pragma warning restore CS4014 // Protože se toto volání neočekává, vykonávání aktuální metody pokračuje před dokončením volání.
            }
            catch
            {

            }

        }


        public async Task AktualizovatZpravyDleOffsetovehoTokenu()
        {

            string UrlSyncFiltrovana = "https://" + matrixServer + "/_matrix/client/r0/sync?since=" + tokenProOfsetSynchronizace; // ?filter={\"room\":{\"timeline\":{\"limit\":2},\"state\":{\"lazy_load_members\":true}}}
            var aaa = await NacistStrankuRestApi(UrlSyncFiltrovana);
            MatrixSyncOdpoved matrixSyncOdpoved = JsonConvert.DeserializeObject<MatrixSyncOdpoved>(aaa);
            bool provestNovyVyber = false;

            if (matrixSyncOdpoved?.Rooms?.Join?.Count != null)
            {
                foreach (var jednaAktualizovanaMistnost in matrixSyncOdpoved.Rooms.Join)
                {
                    if (jednaAktualizovanaMistnost.Value.Timeline.Events.Count != 0)
                    {
                        foreach (Event jedenChatMatrix_udalost in jednaAktualizovanaMistnost.Value.Timeline.Events)
                        {
                            MatrixDatabaze.Instance.VlozitUdalostDoDatabaze(new MatrixDatabaze_Udalost
                            {
                                CasoveRazitko = jedenChatMatrix_udalost.OriginServerTs,
                                Druh = jedenChatMatrix_udalost.Type,
                                IdMistnosti = jednaAktualizovanaMistnost.Key,
                                IdUdalosti = jedenChatMatrix_udalost.EventId,
                                Odesilatel = jedenChatMatrix_udalost.Sender,
                                ObsahJSON = jedenChatMatrix_udalost.ContentJson.ToString()
                            });

                            if (jedenChatMatrix_udalost.Type == "m.room.message" && jedenChatMatrix_udalost.ContentJson?.GetValue("msgtype")?.ToString() == "m.text")
                            {
                                // TODO TAKY AKTIVNÍ KONVERZACE


                                MatrixDatabaze_Mistnost hledaniAktivniSeznamChatu = SeznamChatu.FirstOrDefault(e => e.IdMistnosti == jednaAktualizovanaMistnost.Key);


                                MatrixDatabaze.Instance.VlozitMistnostDoDatabaze(new MatrixDatabaze_Mistnost
                                {
                                    IdMistnosti = jednaAktualizovanaMistnost.Key,
                                    CasovaZnamkaPosledniUdalosti = jedenChatMatrix_udalost.OriginServerTs,
                                    TextPosledniZpravyNahled = jedenChatMatrix_udalost.ContentJson.GetValue("body").ToString()
                                });


                                MatrixDatabaze.Instance.VlozitUdalostDoDatabaze(new MatrixDatabaze_Udalost
                                {
                                    CasoveRazitko = jedenChatMatrix_udalost.OriginServerTs,
                                    Druh = jedenChatMatrix_udalost.Type,
                                    IdMistnosti = jednaAktualizovanaMistnost.Key,
                                    IdUdalosti = jedenChatMatrix_udalost.EventId,
                                    Odesilatel = jedenChatMatrix_udalost.Sender,
                                    ObsahJSON = jedenChatMatrix_udalost.ContentJson.ToString()
                                });



                                if (hledaniAktivniSeznamChatu != null)
                                {
                                    hledaniAktivniSeznamChatu.TextPosledniZpravyNahled = jedenChatMatrix_udalost.ContentJson.GetValue("body").ToString();
                                    SeznamChatu.Move(SeznamChatu.IndexOf(hledaniAktivniSeznamChatu), 0);
                                }
                                else
                                {
                                    // Není v načteném seznamu – na konci smyčky provedu nový výběr
                                    provestNovyVyber = true;
                                }
                            }
                        }
                        
                    }
                    if (jednaAktualizovanaMistnost.Value.State.Events.Count != 0)
                    {

                    }
                    if (jednaAktualizovanaMistnost.Value.Unread_notifications.Notification_count != 0)
                    {

                    }
                }
            }
            else
            {
                Debug.WriteLine("Zprávy jsou aktuální – nový výběr se neprovede");
            }

            // Provést nový výběr z DB, pokud změna nebyla v aktivně zobrazených položkách
            if (provestNovyVyber == true)
            {
                Debug.WriteLine("Zprávy mimo aktivní zobrazení byly změněny – provede se nový výběr");
                SeznamChatu = await MatrixDatabaze.Instance.VybratVsechnyMistnostizDatabaze();
            }


            // Teď uložíme token pro offset synchronizace (next_batch)
            ApplicationData.Current.LocalSettings.Values["tokenProOfsetSynchronizace"] = matrixSyncOdpoved.NextBatch;
            tokenProOfsetSynchronizace = matrixSyncOdpoved.NextBatch;

        }

        public async Task NacistDataChatu()
        {
            if (ApplicationData.Current.LocalSettings.Values["pristupovyToken"] == null)
            {
                // Není uložen přístupový token -> přihlásit
                Debug.WriteLine("Není uložen přístupový token -> přihlásit");
            }
            else if (ApplicationData.Current.LocalSettings.Values["tokenProOfsetSynchronizace"] == null)
            { // Není uložen token pro offset synchronizace – nebyla provedena prvotní synchronizace
                //StackPanelNacitani_Stav.Text = "Provádění první synchronizace – tato operace může trvat delší dobu";
                Debug.WriteLine("MatrixSluzbaSynchronizace.NacistDataChatu(): Stahování celého symchronizačního souboru");
                await Instance.StahnoutCelySynchronizacniSoubor();
            }
            else
            { // Provedeme do-synchronizování za pomoci tokenu
                Debug.WriteLine("Stahování částečného symchronizačního souboru");

                await Instance.AktualizovatZpravyDleOffsetovehoTokenu();

                //StackPanelNacitani_Stav.Text = "Stahování nových zpráv";
                //await Instance.AktualizovatZpravyDleOffsetovehoTokenu();

                //SeznamChatu = await MatrixDatabaze.Instance.VybratVsechnyMistnostizDatabaze();

#pragma warning disable CS4014 // Protože se toto volání neočekává, vykonávání aktuální metody pokračuje před dokončením volání.
                SpustitSynchronizacniSmycku();
#pragma warning restore CS4014 // Protože se toto volání neočekává, vykonávání aktuální metody pokračuje před dokončením volání.

            }


        }


        public async Task SpustitSynchronizacniSmycku()
        {
            if (synchonizacniSmyckaSpustena)
            {
                return;
            }

            synchonizacniSmyckaSpustena = true;

            while (synchonizacniSmyckaSpustena)
            {
                try
                {
                    await Instance.AktualizovatZpravyDleOffsetovehoTokenu();
                    

                    // 4) Zpracovat rooms.join
                    //ZpracovatZmenyMistnosti(syncResponse.Rooms.Join);

                    // 5) Zpracovat timeline eventy
                    //ZpracovatUdalosti(syncResponse.Rooms.Join);

                    // 6) Aktualizovat UI (ObservableCollection)
                    //AktualizovatCache();

                    await Task.Delay(5000);
                    Debug.WriteLine("Synchonizační smyčka!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    // Log, ale nepřerušit loop
                }
            }
        }


    }


}
