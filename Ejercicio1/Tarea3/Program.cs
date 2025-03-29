using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Enumeración para definir los posibles estados de un paciente.
    public enum EstadoPaciente
    {
        Espera,
        Consulta,
        Finalizado
    }

    // Enumeración para definir las prioridades asignables a un paciente.
    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

    public class Paciente
    {
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public PrioridadPaciente Prioridad { get; set; }

        // Constructor que inicializa el paciente con estado "Espera" y asigna la prioridad.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad)
        {
            this.Id = id;
            this.LlegadaHospital = llegadaHospital;
            this.TiempoConsulta = tiempoConsulta;
            this.Estado = EstadoPaciente.Espera; // Estado inicial es "Espera".
            this.Prioridad = prioridad; // Asigna la prioridad pasada como parámetro.
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4; // Número total de pacientes a simular.
            Random rnd = new Random(); // Generador de números aleatorios para ID, tiempos y prioridades.

            // Se simula la llegada y atención de cada paciente.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1; // Orden secuencial de llegada.
                int llegada = i * 2;               // Cada paciente llega con un retraso escalonado de 2 segundos.
                int tiempoConsulta = rnd.Next(5, 16); // Duración de la consulta aleatoria entre 5 y 15 segundos.
                int id = rnd.Next(1, 101); // Genera un ID aleatorio entre 1 y 100.
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3); // Selecciona una prioridad aleatoria.

                Paciente paciente = new Paciente(id, llegada, tiempoConsulta, prioridad);

                // Se crea una tarea asíncrona para simular la atención del paciente.
                Task tarea = Task.Run(() =>
                {
                    // Simula el retraso de llegada.
                    Thread.Sleep(llegada * 1000);
                    
                    // Muestra el estado inicial del paciente al llegar.
                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Espera. Prioridad: {paciente.Prioridad}. Duración Espera: 0 segundos.");

                    int duracionEspera = 0; // Aquí se podría calcular la duración de espera real si se simula.
                    paciente.Estado = EstadoPaciente.Consulta; // Actualiza el estado a "Consulta".
                    
                    // Muestra el cambio de estado a "Consulta".
                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Consulta. Prioridad: {paciente.Prioridad}. Duración Espera: {duracionEspera} segundos.");

                    // Simula el tiempo de consulta.
                    Thread.Sleep(paciente.TiempoConsulta * 1000);
                    
                    paciente.Estado = EstadoPaciente.Finalizado; // Actualiza el estado a "Finalizado" al concluir.
                    
                    // Muestra el estado final del paciente.
                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Finalizado. Prioridad: {paciente.Prioridad}. Duración Consulta: {paciente.TiempoConsulta} segundos.");
                });

                tareas.Add(tarea); // Se añade la tarea a la lista para su posterior seguimiento.
            }

            // Espera a que todas las tareas finalicen antes de cerrar el programa.
            Task.WaitAll(tareas.ToArray());
            Console.WriteLine("Todas las consultas han finalizado.");
        }
    }
}
