using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using static MatrixUWP.ApiWebKlient;

namespace MatrixUWP
{
    public class KonvertorJedenChatTextZpravy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Dictionary<string, object> contentJedneZpravy = value as Dictionary<string, object>;

            return ZiskatHodnotuDictionary(contentJedneZpravy, "body") ?? "Jiný druh zprávy";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class KonvertorSirkaBublinyZpravy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width)
            {
                return width * 0.7; // 70%
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class VybratDruhZpravyDleObsahu : DataTemplateSelector
    {
        public DataTemplate SablonaZprava_OdUzivatele_Text { get; set; }
        public DataTemplate SablonaZprava_OdUzivatele_Obrazek { get; set; }
        public DataTemplate SablonaZprava_OdUzivatele_Video { get; set; }
        public DataTemplate SablonaZprava_OdUzivatele_Zvuk { get; set; }
        public DataTemplate SablonaZprava_OdUzivatele_Soubor { get; set; }
        public DataTemplate SablonaZprava_OdUzivatele_Smazano { get; set; }



        public DataTemplate SablonaZprava_OdNekohoJineho_Text { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Obrazek { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Video { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Zvuk { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Soubor { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Smazano { get; set; }



        public DataTemplate SablonaZprava_ChybaNacitani { get; set; }
        public DataTemplate SablonaZprava_Clen { get; set; }
        public DataTemplate SablonaZprava_Reakce { get; set; }
        public DataTemplate SablonaZprava_Nezobrazovat { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Event jednaZprava = item as Event;

            if (jednaZprava?.Type != null)
            {
                if (jednaZprava.Type == "m.room.message")
                {
                    string druhZpravy = jednaZprava.ContentJson.GetValue("msgtype").ToString() ?? "m.text";

                    if (jednaZprava.Sender == "@" + MatrixSluzbaSynchronizace.Instance.uzivatelskeJmeno + ":4d2.org")
                    { // Zpráva od uživatele
                        switch (druhZpravy)
                        {
                            case "m.text":
                                return SablonaZprava_OdUzivatele_Text;
                            case "m.image":
                                return SablonaZprava_OdUzivatele_Obrazek;
                            case "m.video":
                                return SablonaZprava_OdUzivatele_Video;
                            case "m.audio":
                                return SablonaZprava_OdUzivatele_Zvuk;
                            case "m.file":
                                return SablonaZprava_OdUzivatele_Soubor;
                            default:
                                return SablonaZprava_OdUzivatele_Text;

                                // Ještě m.room.redaction
                        }
                    }
                    else
                    { // Zpráva od někoho jiného
                        switch (druhZpravy)
                        {
                            case "m.text":
                                return SablonaZprava_OdNekohoJineho_Text;
                            case "m.image":
                                return SablonaZprava_OdNekohoJineho_Obrazek;
                            case "m.video":
                                return SablonaZprava_OdNekohoJineho_Video;
                            case "m.audio":
                                return SablonaZprava_OdNekohoJineho_Zvuk;
                            case "m.file":
                                return SablonaZprava_OdNekohoJineho_Soubor;
                            default:
                                return SablonaZprava_OdNekohoJineho_Text;
                        }
                    }
                }
                else if (jednaZprava.Type == "m.reaction")
                {
                    return SablonaZprava_Reakce;
                }
                else if (jednaZprava.Type == "m.room.member")
                {
                    return SablonaZprava_Clen;
                }
                else if (jednaZprava.Type == "m.room.redaction")
                {
                    return jednaZprava.Sender == "@" + MatrixSluzbaSynchronizace.Instance.uzivatelskeJmeno + ":4d2.org"
                        ? SablonaZprava_OdUzivatele_Smazano
                        : SablonaZprava_OdNekohoJineho_Smazano;
                }
                else
                {
                    return SablonaZprava_ChybaNacitani;
                }
            }
            else
            {
                return SablonaZprava_ChybaNacitani;
            }
        }
    }

    public class KonvertorJedenChatCasOdeslaniZpravy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            long unixoveSekundyPosledniZpravy = (long)value;
            return DateTimeOffset.FromUnixTimeMilliseconds(unixoveSekundyPosledniZpravy).LocalDateTime.ToString("H:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class KonvertorViditelnostiNullJeVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as BitmapImage) == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class KonvertorJmenoUzivatelePodleMatrixId : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string matrixIdUzivatele = (string)value;
            string zobrazovaneJmeno = StrankaJedenChat.chatKterySeMaZobrazit.ClenoveChatu.Find(x => x.MatrixIdUzivatele == matrixIdUzivatele)?.ZobrazovaneJmeno;

            if (zobrazovaneJmeno != null)
            {
                return zobrazovaneJmeno;
            }
            else
            {
                return matrixIdUzivatele;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class KonvertorSeznamChatuCasPosledniZpravy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            long unixoveSekundyPosledniZpravy = (long)value;
            DateTime dateTimePosledniZpravy = DateTimeOffset.FromUnixTimeMilliseconds(unixoveSekundyPosledniZpravy).LocalDateTime;

            if (dateTimePosledniZpravy.Day == DateTime.Now.Day && dateTimePosledniZpravy.Month == DateTime.Now.Month && dateTimePosledniZpravy.Year == DateTime.Now.Year) // Je to dneska, dát jenom čas
            {
                return dateTimePosledniZpravy.ToString("H:mm");
            }
            else if (dateTimePosledniZpravy.Day == DateTime.Now.AddDays(-1).Day && dateTimePosledniZpravy.Month == DateTime.Now.AddDays(-1).Month && dateTimePosledniZpravy.Year == DateTime.Now.AddDays(-1).Year)
            {
                return "včera v " + dateTimePosledniZpravy.ToString("H:mm");
            }
            else
            {
                return dateTimePosledniZpravy.ToString("d. M. yyyy");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public class KonvertorSeznamChatuTextPosledniZpravyzJson : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            JObject posledniZpravaJson = (JObject)value;
            return posledniZpravaJson?.GetValue("body")?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
