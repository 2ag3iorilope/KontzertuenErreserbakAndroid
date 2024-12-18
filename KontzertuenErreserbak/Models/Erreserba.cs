using SQLite;

namespace KontzertuenErreserbak.Models
{
    /// <summary>
    /// Erreserbak taula.
    /// </summary>
    public class Erreserba
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int id_kontzertua { get; set; } // Erlazioa

        public string DNI { get; set; }

        public string Abizena { get; set; }

        public string Izena { get; set; }

        public int Kantitatea { get; set; }
    }
}