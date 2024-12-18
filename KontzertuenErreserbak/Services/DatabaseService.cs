using SQLite;
using KontzertuenErreserbak.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KontzertuenErreserbak.Services
{
    public class DatabaseService
    {
        // Datu-basearekin konexioa.
        private readonly SQLiteAsyncConnection _database;

        // Blokeoak kontrolatzeko objektua (segurtasun hobea hurreratze asinkronoan).
        private static readonly object dbLock = new object();

        /// <summary>
        /// Datu-basearen bidea hartu eta taulak sortuko ditu
        /// </summary>
        /// <param name="dbPath"></param>
        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<Kontzertua>().Wait();
            _database.CreateTableAsync<Erreserba>().Wait();
        }

        /// <summary>
        /// Kontzertu bat gordetzeko funtzioa
        /// </summary>
        /// <param name="concierto"></param>
        /// <returns></returns>
        public Task<int> SaveConciertoAsync(Kontzertua concierto)
        {
            lock (dbLock)
            {
                return _database.InsertAsync(concierto);
            }
        }

        /// <summary>
        /// Datu-basean gordetako kontzertu guztiak lortzeko funtzioa
        /// </summary>
        /// <returns></returns>
        public Task<List<Kontzertua>> GetConciertosAsync()
        {
            lock (dbLock)
            {
                return _database.Table<Kontzertua>().ToListAsync();
            }
        }

        /// <summary>
        /// Erreserba bat gordetzeko funtzioa
        /// </summary>
        /// <param name="reserva"></param>
        /// <returns></returns>
        public Task<int> SaveReservaAsync(Erreserba reserva)
        {
            lock (dbLock)
            {
                // Erreserba berria datu-basean gehitu
                return _database.InsertAsync(reserva);
            }
        }

        /// <summary>
        /// Kontzertu bateko erreserbak lortzeko funtzioa
        /// </summary>
        /// <param name="conciertoId"></param>
        /// <returns></returns>
        public Task<List<Erreserba>> GetReservasByConciertoAsync(int conciertoId)
        {
            lock (dbLock)
            {
                // Erreserba taulatik kontzertu horri dagozkion erreserbak bilatu
                return _database.Table<Erreserba>()
                                .Where(r => r.id_kontzertua == conciertoId)
                                .ToListAsync();
            }
        }

        /// <summary>
        /// Kontzertu jakin bateko guztizko erreserben kopurua kalkulatzeko funtzioa
        /// </summary>
        /// <param name="conciertoId"></param>
        /// <returns></returns>
        public Task<int> SumReservasByConciertoAsync(int conciertoId)
        {
            lock (dbLock)
            {
                // Erreserba taulan kontzertu horren guztizko kantitatea kalkulatu
                return _database.ExecuteScalarAsync<int>(
                    "SELECT IFNULL(SUM(Kantitatea), 0) FROM Erreserba WHERE id_kontzertua = ?", conciertoId);
            }
        }

        //Aldaketa Kontzertuak Sortu bestela beti sold out agertzen da eta ez da botoia inoiz aktibatuko
        public async Task AddSampleConcertsAsync()
        {
            lock (dbLock)
            {
                // Crear conciertos de ejemplo con las ciudades especificadas
                var concierto1 = new Kontzertua { Herria = "Bilbao", Data = "2024-12-25", Aforoa = 100 };
                var concierto2 = new Kontzertua { Herria = "Barcelona", Data = "2024-12-30", Aforoa = 50 };
                var concierto3 = new Kontzertua { Herria = "Madrid", Data = "2024-12-31", Aforoa = 200 };

                // Añadir conciertos a la base de datos
                _database.InsertAsync(concierto1).Wait();
                _database.InsertAsync(concierto2).Wait();
                _database.InsertAsync(concierto3).Wait();
            }
        }
        //Aldaketa Erreserbak Sortu bestela beti sold out agertzen da eta ez da botoia inoiz aktibatuko
        public async Task AddSampleReservationsAsync()
        {
            lock (dbLock)
            {
                // Crear algunas reservas
                var reserva1 = new Erreserba { id_kontzertua = 1, Kantitatea = 50 };  // 50 reservas para el concierto de Bilbao
                var reserva2 = new Erreserba { id_kontzertua = 2, Kantitatea = 30 };  // 30 reservas para el concierto de Barcelona
                var reserva3 = new Erreserba { id_kontzertua = 3, Kantitatea = 150 }; // 150 reservas para el concierto de Madrid

                // Añadir reservas a la base de datos
                _database.InsertAsync(reserva1).Wait();
                _database.InsertAsync(reserva2).Wait();
                _database.InsertAsync(reserva3).Wait();
            }
        }

        /// <summary>
        /// Kontzertu baten aforoa lortzeko funtzioa
        /// </summary>
        /// <param name="conciertoId"></param>
        /// <returns></returns>
        public Task<int> GetAforoByConciertoIdAsync(int conciertoId)
        {
            lock (dbLock)
            {
                // Kontzertua taulatik kontzertuaren aforoa bilatu
                return _database.ExecuteScalarAsync<int>(
                    "SELECT Aforoa FROM Kontzertua WHERE Id = ?", conciertoId);
            }
        }
    }
}
