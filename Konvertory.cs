using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        public DataTemplate SablonaZprava_OdNekohoJineho_Text { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Obrazek { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Video { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Zvuk { get; set; }
        public DataTemplate SablonaZprava_OdNekohoJineho_Soubor { get; set; }


        public DataTemplate SablonaZprava_ChybaNacitani { get; set; }
        public DataTemplate SablonaZprava_Clen { get; set; }
        public DataTemplate SablonaZprava_Reakce { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Event jednaZprava = item as Event;

            if (jednaZprava?.Type != null)
            {
                if (jednaZprava.Type == "m.room.message")
                {
                    string druhZpravy = ZiskatHodnotuDictionary(jednaZprava.Content, "msgtype") ?? "m.text";

                    if (jednaZprava.Sender == "@" + StrankaChaty.uzivatelskeJmeno + ":4d2.org")
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
}
