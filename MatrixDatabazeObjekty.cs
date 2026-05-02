using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixUWP
{
    public class MatrixDatabazeObjekty
    {

        public class MatrixDatabaze_Udalost
        {
            public string IdUdalosti { get; set; }
            public string IdMistnosti { get; set; }
            public string Odesilatel { get; set; }
            public long CasoveRazitko { get; set; }
            public string Druh { get; set; }
            public string ObsahJSON { get; set; }
            public long IndexVMistnosti { get; set; }
        }

        public class MatrixDatabaze_Mistnost
        {
            public string IdMistnosti { get; set; }
            public string Nazev { get; set; }
            public string UrlObrazku { get; set; }
            public long CasovaZnamkaPosledniUdalosti { get; set; }
            public string TextPosledniZpravyNahled { get; set; }
            public int PocetNeprectenych { get; set; }
        }

        public class MatrixDatabaze_Stav
        {
            public int IdStavu { get; set; }
            public string IdMistnosti { get; set; }
            public string Druh { get; set; }
            public string StateKey { get; set; }
            public string ObsahJSON { get; set; }
        }


    }
}
