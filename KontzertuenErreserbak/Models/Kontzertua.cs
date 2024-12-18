using SQLite;

namespace KontzertuenErreserbak.Models
{
    /// <summary>
    /// Kontzertuak taula.
    /// </summary>
    public class Kontzertua
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Herria { get; set; }

        public string Data { get; set; }

        public int Aforoa { get; set; }
    }
}
