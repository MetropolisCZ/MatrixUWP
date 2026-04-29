using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using static MatrixUWP.MatrixDatabazeObjekty;

namespace MatrixUWP
{
    public sealed class MatrixDatabaze
    {
        private static MatrixDatabaze _instance;
        private readonly string _cesta;

        public static MatrixDatabaze Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MatrixDatabaze();

                return _instance;
            }
        }

        private MatrixDatabaze()
        {
            _cesta = Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                "matrix.db"
            );
        }

        // ---------------------------------------------------------
        // Inicializace databáze (volat v App.xaml.cs → OnLaunched)
        // ---------------------------------------------------------
        public void Inicializovat()
        {
            using (var spojeni = new SqliteConnection("Data Source=" + _cesta))
            {
                spojeni.Open();

                using (var prikaz = spojeni.CreateCommand())
                {
                    prikaz.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS Udalosti (
                        IdUdalosti TEXT PRIMARY KEY,
                        IdMistnosti TEXT NOT NULL,
                        Odesilatel TEXT NOT NULL,
                        CasoveRazitko INTEGER NOT NULL,
                        Druh TEXT NOT NULL,
                        ObsahJSON TEXT NOT NULL,
                        IndexVMistnosti INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Mistnosti (
                        IdMistnosti TEXT PRIMARY KEY,
                        Nazev TEXT,
                        UrlObrazku TEXT,
                        CasovaZnamkaPosledniUdalosti INTEGER,
                        PocetNeprectenych INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS Stavy (
                        IdStavu INTEGER PRIMARY KEY AUTOINCREMENT,
                        IdMistnosti TEXT,
                        Druh TEXT,
                        StateKey TEXT,
                        ObsahJSON TEXT
                    );
                    ";

                    prikaz.ExecuteNonQuery();
                }
            }
        }

        // ---------------------------------------------------------
        // Vložit událost (zprávu)
        // ---------------------------------------------------------
        public void VlozitUdalostDoDatabaze(MatrixDatabaze_Udalost u)
        {
            using (var spojeni = new SqliteConnection("Data Source=" + _cesta))
            {
                spojeni.Open();

                using (var prikaz = spojeni.CreateCommand())
                {
                    prikaz.CommandText =
                    @"
                    INSERT OR IGNORE INTO Udalosti
                    (IdUdalosti, IdMistnosti, Odesilatel, CasoveRazitko, Druh, ObsahJSON, IndexVMistnosti)
                    VALUES ($id, $mistnost, $odesilatel, $cas, $druh, $obsah, $index);
                    ";

                    prikaz.Parameters.AddWithValue("$id", u.IdUdalosti);
                    prikaz.Parameters.AddWithValue("$mistnost", u.IdMistnosti);
                    prikaz.Parameters.AddWithValue("$odesilatel", u.Odesilatel);
                    prikaz.Parameters.AddWithValue("$cas", u.CasoveRazitko);
                    prikaz.Parameters.AddWithValue("$druh", u.Druh);
                    prikaz.Parameters.AddWithValue("$obsah", u.ObsahJSON);
                    prikaz.Parameters.AddWithValue("$index", u.IndexVMistnosti);

                    prikaz.ExecuteNonQuery();
                }
            }
        }

        // ---------------------------------------------------------
        // Vložit nebo aktualizovat místnost
        // ---------------------------------------------------------
        public void VlozitMistnostDoDatabaze(MatrixDatabaze_Mistnost m)
        {
            using (var spojeni = new SqliteConnection("Data Source=" + _cesta))
            {
                spojeni.Open();

                using (var prikaz = spojeni.CreateCommand())
                {
                    prikaz.CommandText =
                    @"
                    INSERT OR REPLACE INTO Mistnosti
                    (IdMistnosti, Nazev, UrlObrazku, CasovaZnamkaPosledniUdalosti, PocetNeprectenych)
                    VALUES ($id, $nazev, $url, $cas, $neprectene);
                    ";

                    prikaz.Parameters.AddWithValue("$id", m.IdMistnosti);
                    prikaz.Parameters.AddWithValue("$nazev", m.Nazev);
                    prikaz.Parameters.AddWithValue("$url", m.UrlObrazku);
                    prikaz.Parameters.AddWithValue("$cas", m.CasovaZnamkaPosledniUdalosti);
                    prikaz.Parameters.AddWithValue("$neprectene", m.PocetNeprectenych);

                    prikaz.ExecuteNonQuery();
                }
            }
        }

        // ---------------------------------------------------------
        // Vložit stavový event
        // ---------------------------------------------------------
        public void VlozitStavDoDatabaze(MatrixDatabaze_Stav s)
        {
            using (var spojeni = new SqliteConnection("Data Source=" + _cesta))
            {
                spojeni.Open();

                using (var prikaz = spojeni.CreateCommand())
                {
                    prikaz.CommandText =
                    @"
                    INSERT INTO Stavy
                    (IdMistnosti, Druh, StateKey, ObsahJSON)
                    VALUES ($mistnost, $druh, $statekey, $obsah);
                    ";

                    prikaz.Parameters.AddWithValue("$mistnost", s.IdMistnosti);
                    prikaz.Parameters.AddWithValue("$druh", s.Druh);
                    prikaz.Parameters.AddWithValue("$statekey", s.StateKey);
                    prikaz.Parameters.AddWithValue("$obsah", s.ObsahJSON);

                    prikaz.ExecuteNonQuery();
                }
            }
        }
    }
}
