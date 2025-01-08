using KontzertuenErreserbak.Models;
using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System.Diagnostics;


namespace KontzertuenErreserbak
{
    public partial class MainPage : ContentPage
    {
        private int selectedConciertoId = 0;

        public MainPage()
        {
            InitializeComponent();

            _ = App.Database.AddSampleConcertsAsync();
            _ = App.Database.AddSampleReservationsAsync();
            UpdateReservasLabel(1);


            IzenaEntry.TextChanged += izenaOndoSartu;
            AbizenaEntry.TextChanged += abizenaOndoSartu;
            DniEntry.TextChanged += dniOndoSartu;
            KantitateaEntry.TextChanged += kantitateaOndoSartu;

            BilbaoRadioButton.CheckedChanged += OnRadioButtonCheckedChanged;
            BartzelonaRadioButton.CheckedChanged += OnRadioButtonCheckedChanged;
            MadridRadioButton.CheckedChanged += OnRadioButtonCheckedChanged;

            btnErreserbatu.IsEnabled = false;
        }

        /// <summary>
        /// Izena eremuan bakarrik letrak sartzea baimenduko duen funtzioa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void izenaOndoSartu(object sender, TextChangedEventArgs e)
        {
            IzenaEntry.Text = letrakBakarrikOnartu(e.NewTextValue);
        }

        /// <summary>
        /// Abizena eremuan bakarrik letrak sartzea baimenduko duen funtzioa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void abizenaOndoSartu(object sender, TextChangedEventArgs e)
        {
            AbizenaEntry.Text = letrakBakarrikOnartu(e.NewTextValue);
        }

        /// <summary>
        /// Letrak bakarrik onartuko duen funtzioa.
        /// Metodo hau izenaOndoSartu eta abizenaOndoSartu metodoetan erabiltzen da.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string letrakBakarrikOnartu(string input)
        {
            return Regex.Replace(input, "[^a-zA-Z]", ""); // Letrak ez diren beste guztia ezabatzen du.
        }

        /// <summary>
        /// DNI eremuan bakarrik zenbakiak sartzea baimenduko duen funtzioa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dniOndoSartu(object sender, TextChangedEventArgs e)
        {
            
            string dni = DniEntry.Text;

            if (!NIF.BaliozkatuCIF(dni) && !NIF.BaliozkatuNIF(dni))
            {
                DniEntry.TextColor = Colors.Red;
            }
            else

                DniEntry.TextColor = Colors.Green;
        }

        /// <summary>
        /// Kantitatea eremuan bakarrik zenbakiak sartzea baimenduko duen funtzioa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void kantitateaOndoSartu(object sender, TextChangedEventArgs e)
        {
            KantitateaEntry.Text = FilterTextOnlyNumbers(e.NewTextValue);
            erreserbaFormularioaBeteta();
        }

        /// <summary>
        /// Zenbakiak bakarrik onartuko dituen funtzioa.
        /// Metodo hau dniOndoSartu eta kantitateaOndoSartu metodoetan erabiltzen da.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FilterTextOnlyNumbers(string input)
        {
            return Regex.Replace(input, "[^0-9]", ""); // Zenbakiak ez diren beste guztia ezabatzen du.
        }

        /// <summary>
        /// Eremu guztiak betetzen ditugunean botoia gaituko da eta erreserba egingo da.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnErreserbatu_Clicked(object sender, EventArgs e)
        {
            // Obtener la cantidad total de reservas para el concierto seleccionado
            int erreserbaTotalaKant = await App.Database.SumReservasByConciertoAsync(selectedConciertoId);

            // Mensajes de depuración para verificar los valores
            Debug.WriteLine($"Reserva Total Cantidad: {erreserbaTotalaKant}");

            // Verificar si la entrada de cantidad es válida
            if (!int.TryParse(KantitateaEntry.Text, out int eskatutakoKantitatea) || eskatutakoKantitatea <= 0)
            {
                await DisplayAlert("Errorea", "Sarrera kantitate baliogabea.", "Ados");
                return;
            }

            try
            {
                var reserva = new Erreserba
                {
                    id_kontzertua = selectedConciertoId,
                    DNI = string.IsNullOrWhiteSpace(DniEntry.Text) ? "DNI-a gabe" : DniEntry.Text,
                    Abizena = string.IsNullOrWhiteSpace(AbizenaEntry.Text) ? "Abizen gabe" : AbizenaEntry.Text,
                    Izena = string.IsNullOrWhiteSpace(IzenaEntry.Text) ? "Izen gabe" : IzenaEntry.Text,
                    Kantitatea = eskatutakoKantitatea
                };

                // Guardar la reserva en la base de datos
                await App.Database.SaveReservaAsync(reserva);

                // Actualizar la cantidad total de reservas para el concierto seleccionado
                erreserbaTotalaKant = await App.Database.SumReservasByConciertoAsync(selectedConciertoId);

                // Mensajes de depuración para verificar los valores actualizados
                Debug.WriteLine($"Reserva Total Cantidad Actualizada: {erreserbaTotalaKant}");

                // Actualizar la etiqueta para mostrar el número total de reservas
                ErreserbaCountLabel.Text = $"Reservas totales: {erreserbaTotalaKant}";

                // Mostrar mensaje de éxito al usuario
                await DisplayAlert("Ongi!", "Erreserba ondo egin da", "Ados");

                // Limpiar los campos de entrada
                garbituImput();

                // Habilitar el botón de reserva si todos los campos están completos
                erreserbaFormularioaBeteta();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errorea", $"Ezin izan da erreserba egin: {ex.Message}", "Ados");
            }
        }

        /// <summary>
        /// Erreserba egiterakoan datuak sartzeko eremuak garbituko dira.
        /// </summary>
        private void garbituImput()
        {
            IzenaEntry.Text = string.Empty;
            AbizenaEntry.Text = string.Empty;
            DniEntry.Text = string.Empty;
            KantitateaEntry.Text = string.Empty;

            btnErreserbatu.IsEnabled = false;
            selectedConciertoId = 0;

            BilbaoRadioButton.IsChecked = false;
            BartzelonaRadioButton.IsChecked = false;
            MadridRadioButton.IsChecked = false;

            ErreserbaCountLabel.Text = "";
        }

        /// <summary>
        /// Aplikaziotik ateratzeko botoia.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAtera_Clicked(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }


         async void UpdateReservasLabel(int conciertoId)
        {
            int reservasDisponibles = await App.Database.GetAvailableReservationsAsync(conciertoId);

            if (reservasDisponibles > 0)
            {
                ErreserbaCountLabel.Text = $"Reservas disponibles: {reservasDisponibles}";
            }
            else
            {
                ErreserbaCountLabel.Text = "Sold out";

            }
        }



        /// <summary>
        /// Erreserba formularioa beteta dagoen egiaztatuko duen funtzioa.
        /// </summary>
        private void erreserbaFormularioaBeteta()
        {
            // Mensajes de depuración para verificar los valores de entrada
            Debug.WriteLine($"Izena: {IzenaEntry.Text}");
            Debug.WriteLine($"Abizena: {AbizenaEntry.Text}");
            Debug.WriteLine($"DNI: {DniEntry.Text} (Length: {DniEntry.Text?.Length})");
            Debug.WriteLine($"Kantitatea: {KantitateaEntry.Text}");
            Debug.WriteLine($"selectedConciertoId: {selectedConciertoId}");

            bool isFormComplete =
                !string.IsNullOrWhiteSpace(IzenaEntry.Text) &&
                !string.IsNullOrWhiteSpace(AbizenaEntry.Text) &&
                !string.IsNullOrWhiteSpace(DniEntry.Text) &&
                DniEntry.Text.Length == 9 &&
                !string.IsNullOrWhiteSpace(KantitateaEntry.Text) &&
                selectedConciertoId != 0;

            // Habilitar el botón de reserva si todos los campos están completos
            btnErreserbatu.IsEnabled = isFormComplete;
        }

        /// <summary>
        /// Kontzertuen aukeretan aukeratutako kontzertuaren IDa gordeko duen funtzioa.
        /// Ondoren erreserba kopurua kalkulatzeko erabiliko da.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var radioButton = sender as RadioButton;
                selectedConciertoId = int.Parse(radioButton.Value.ToString());

                // Obtener el aforo total del concierto seleccionado
                int aforoTotala = await App.Database.GetAforoByConciertoIdAsync(selectedConciertoId);
                // Obtener la cantidad total de reservas para el concierto seleccionado
                int erreserbaTotalaKant = await App.Database.SumReservasByConciertoAsync(selectedConciertoId);

                // Mensajes de depuración para verificar los valores
                Debug.WriteLine($"Aforo Total: {aforoTotala}");
                Debug.WriteLine($"Reserva Total Cantidad: {erreserbaTotalaKant}");

                // Actualizar la etiqueta para mostrar el número total de reservas
                ErreserbaCountLabel.Text = $"Reservas totales: {erreserbaTotalaKant}";

                // Formularioa beteta dagoen egiaztatu
                erreserbaFormularioaBeteta();
            }
        }

    }
}
