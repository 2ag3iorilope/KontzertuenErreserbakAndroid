using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontzertuenErreserbak
{
    static class NIF
    {
        /// <summary>
        /// NIF letra kalkulatu zenbakiak oinarri hartuta
        /// </summary>
        /// <param name="nifOinarria">NIF zenbaki eta letra</param>
        /// <returns>NIFari dagokion letra itzultzen du</returns>
        private static string KalkulatuNIF(string nifOinarria)
        {
            const string letraAukera = "TRWAGMYFPDXBNJZSQVHLCKE";
            const string zenbakiAukera = "0123456789";
            int a, b, d, NIF;
            StringBuilder sb = new StringBuilder();

            nifOinarria = Strings.Trim(nifOinarria);
            if (Strings.Len(nifOinarria) == 0)
                return "";

            // Zenbakiak soilik utzi
            for (int i = 0; i <= nifOinarria.Length - 1; i++)
            {
                if (zenbakiAukera.IndexOf(nifOinarria[i]) > -1)
                    sb.Append(nifOinarria[i]);
            }

            nifOinarria = sb.ToString();
            a = 0;
            NIF = System.Convert.ToInt32(Conversion.Val(nifOinarria));

            do
            {
                b = System.Convert.ToInt32(Conversion.Int(NIF / (double)24));
                d = NIF - (24 * b);
                a = a + d;
                NIF = b;
            }
            while (b != 0);


            b = System.Convert.ToInt32(Conversion.Int(a / (double)23));
            d = a - (23 * b);

            return nifOinarria + Strings.Mid(letraAukera, System.Convert.ToInt32(d + 1), 1);
        }

        /// <summary>
        /// Emandako CIFa egokia den ala ez frogatzen du
        /// </summary>
        /// <param name="cif">CIF zenbakia letra eta guzti</param>
        /// <returns>True, zuzena bada sartutako CIFa. False, bestela</returns>
        public static bool BaliozkatuCIF(string cif)
        {
            string letra;
            string zenbakia;
            string digitua;
            string digituAux;
            int auxZenbaki;
            int i;
            int batura = 0;
            string letraAukera = "ABCDEFGHKLMPQSX";

            cif = Strings.UCase(cif);

            if (Strings.Len(cif) < 9 || !Information.IsNumeric(Strings.Mid(cif, 2, 7)))
                return false;

            letra = Strings.Mid(cif, 1, 1);      // CIFaren letra
            zenbakia = Strings.Mid(cif, 2, 7);   // Kontrol kodigoa
            digitua = Strings.Mid(cif, 9);       // CIFari lehen eta azken posizioak kenduta

            if (letraAukera.IndexOf(letra) == 0)
                return false;

            for (i = 1; i <= 7; i++)
            {
                if (i % 2 == 0)
                    batura = batura + System.Convert.ToInt32(Strings.Mid(zenbakia, i, 1));
                else
                {
                    auxZenbaki = System.Convert.ToInt32(Strings.Mid(zenbakia, i, 1)) * 2;
                    batura = batura + (auxZenbaki / 10) + (auxZenbaki % 10);
                }
            }
            batura = (10 - (batura % 10)) % 10;

            switch (letra)
            {
                case "K":
                case "P":
                case "Q":
                case "S":
                    {
                        batura = batura + 64;
                        digituAux = Strings.Chr(batura).ToString();
                        break;
                    }

                case "X":
                    {
                        digituAux = Strings.Mid(KalkulatuNIF(zenbakia), 8, 1);
                        break;
                    }

                default:
                    {
                        byte[] hex = BitConverter.GetBytes(batura);
                        digituAux = System.Convert.ToHexString(hex);
                        break;
                    }
            }

            if (digitua == digituAux)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Emandako NIFa egokia den ala ez frogatzen du
        /// </summary>
        /// <param name="nif">NIF zenabkia letra eta guzti</param>
        /// <returns>True, egokia bada emandako NIFa. False, bestela</returns>
        public static bool BaliozkatuNIF(string nif)
        {
            string aux;

            // letra maiuskulan jarri
            nif = nif.ToUpper();

            // aux, letrarik gabeko NIFa da
            aux = nif.Substring(0, nif.Length - 1);


            if (aux.Length >= 7 && Information.IsNumeric(aux))
            {
                // Kalkulatu NIF letra, jaso dugunarekin konparatzeko
                aux = KalkulatuNIF(aux);
            }
            else
            {
                return false;
            }

            if (nif != aux)
            {
                return false;
            }

            return true;
        }
    }
}
