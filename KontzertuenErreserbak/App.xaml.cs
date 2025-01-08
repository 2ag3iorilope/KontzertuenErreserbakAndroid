using System.Diagnostics;
using System.IO;
using KontzertuenErreserbak.Services;

namespace KontzertuenErreserbak
{
    public partial class App : Application
    {
        private static DatabaseService _database;

        public static DatabaseService Database
        {
            get
            {
                if (_database == null)
                {
                    // Ruta mahaigaina.
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    // Datu basea fitxategiaren izena.
                    //Aldaketa: Ruta aldatu dut Androidean ez baita desktop
                    string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Kontzertuak.db3");

                    Debug.WriteLine($"Base de datos utilizada: {dbPath}");
                    // Datu basea iniziatu
                    _database = new DatabaseService(dbPath);
                }
                return _database;
            }
        }

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
