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
        public async Task<List<Erreserba>> GetReservasByConciertoAsync(int conciertoId)
        {
            return await _database.Table<Erreserba>()
                                  .Where(r => r.id_kontzertua == conciertoId)
                                  .ToListAsync();
        }
        public async Task<int> GetAvailableReservationsAsync(int conciertoId)
        {
            // Obtener el aforo del concierto
            int aforo = await GetAforoByConciertoIdAsync(conciertoId);

            // Obtener la suma de las reservas para el concierto
            int totalReservas = await SumReservasByConciertoAsync(conciertoId);

            // Calcular las reservas disponibles
            int reservasDisponibles = aforo - totalReservas;

            return reservasDisponibles;
        }

        /// <summary>
        /// Kontzertu jakin bateko guztizko erreserben kopurua kalkulatzeko funtzioa
        /// </summary>
        /// <param name="conciertoId"></param>
        /// <returns></returns>
        public Task<int> SumReservasByConciertoAsync(int conciertoId)
        {
           
            {
                // Erreserba taulan kontzertu horren guztizko kantitatea kalkulatu
                return _database.ExecuteScalarAsync<int>(
                   "SELECT IFNULL(SUM(Kantitatea), 0) FROM Erreserba WHERE id_kontzertua = ?", conciertoId);
            }
        }

        //Aldaketa Kontzertuak Sortu bestela beti sold out agertzen da eta ez da botoia inoiz aktibatuko
        public async Task AddSampleConcertsAsync()
        {
            var concierto1 = new Kontzertua { Herria = "Bilbao", Data = "2024-12-25", Aforoa = 100 };
            var concierto2 = new Kontzertua { Herria = "Barcelona", Data = "2024-12-30", Aforoa = 50 };
            var concierto3 = new Kontzertua { Herria = "Madrid", Data = "2024-12-31", Aforoa = 200 };

            await _database.InsertAsync(concierto1);
            await _database.InsertAsync(concierto2);
            await _database.InsertAsync(concierto3);
        }

        public async Task AddSampleReservationsAsync()
        {
            var reserva1 = new Erreserba { id_kontzertua = 1, Kantitatea = 50 };
            var reserva2 = new Erreserba { id_kontzertua = 2, Kantitatea = 30 };
            var reserva3 = new Erreserba { id_kontzertua = 3, Kantitatea = 150 };

            if (await CanAddReservation(reserva1)) await _database.InsertAsync(reserva1);
            if (await CanAddReservation(reserva2)) await _database.InsertAsync(reserva2);
            if (await CanAddReservation(reserva3)) await _database.InsertAsync(reserva3);
        }

        private async Task<bool> CanAddReservation(Erreserba reserva)
        {
            int aforoTotala = await GetAforoByConciertoIdAsync(reserva.id_kontzertua);
            int erreserbaTotalaKant = await SumReservasByConciertoAsync(reserva.id_kontzertua);
            return (erreserbaTotalaKant + reserva.Kantitatea) <= aforoTotala;
        }
        

        /// <summary>
        /// Kontzertu baten aforoa lortzeko funtzioa
        /// </summary>
        /// <param name="conciertoId"></param>
        /// <returns></returns>
        public Task<int> GetAforoByConciertoIdAsync(int conciertoId)
        {
           
            {
                // Kontzertua taulatik kontzertuaren aforoa bilatu
                return _database.ExecuteScalarAsync<int>(
                    "SELECT Aforoa FROM Kontzertua WHERE Id = ?", conciertoId);
            }
        }
    }
}
