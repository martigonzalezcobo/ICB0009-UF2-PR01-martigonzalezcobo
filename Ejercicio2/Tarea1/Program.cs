using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Definición de los estados que puede tener un paciente durante su atención.
    public enum EstadoPaciente
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Finalizado
    }

    // Definición de las prioridades asignables a los pacientes.
    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

    // Clase que representa a un paciente, con sus datos y requerimientos.
    public class Paciente
    {
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public PrioridadPaciente Prioridad { get; set; }
        public bool RequiereDiagnostico { get; set; } // Indica si el paciente necesita diagnóstico.

        // Constructor que inicializa el paciente con estado inicial "EsperaConsulta".
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta; // Estado inicial asignado.
            Prioridad = prioridad;
            RequiereDiagnostico = requiereDiagnostico;
        }
    }

    class Program
    {
        // Semaphore para limitar la cantidad de diagnósticos simultáneos a 2.
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);

        // Método asíncrono que simula la atención completa de un paciente.
        static async Task AtenderPaciente(Paciente paciente, int ordenLlegada)
        {
            // Muestra la llegada del paciente y sus datos iniciales.
            Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: {paciente.Estado}. Prioridad: {paciente.Prioridad}.");

            paciente.Estado = EstadoPaciente.Consulta; // Actualiza el estado a "Consulta".
            Console.WriteLine($"Paciente {paciente.Id}. Entra en consulta.");

            // Simula la duración de la consulta (convertimos segundos a milisegundos).
            await Task.Delay(paciente.TiempoConsulta * 1000);

            if (paciente.RequiereDiagnostico)
            {
                // Cambia el estado a "EsperaDiagnostico" si es necesario.
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id}. Requiere diagnóstico.");
                
                // Espera a que haya una máquina de diagnóstico disponible.
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    Console.WriteLine($"Paciente {paciente.Id}. Inicia diagnóstico.");
                    // Simula el proceso de diagnóstico durante 15 segundos.
                    await Task.Delay(15000);
                }
                finally
                {
                    // Libera la máquina para el siguiente paciente.
                    maquinasDiagnostico.Release();
                }
            }
            paciente.Estado = EstadoPaciente.Finalizado; // Finaliza la atención.
            // Muestra el tiempo total de atención, sumando consulta y diagnóstico si corresponde.
            Console.WriteLine($"Paciente {paciente.Id}. Finalizado. Duración {(paciente.RequiereDiagnostico ? "Diagnóstico + Consulta" : "Consulta")}: {paciente.TiempoConsulta + (paciente.RequiereDiagnostico ? 15 : 0)} segundos.");
        }

        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4; // Número total de pacientes a simular.
            Random rnd = new Random();

            // Se crean tareas para simular la llegada escalonada de cada paciente.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;
                int llegada = i * 2000;            // Simula retraso en la llegada (2 segundos por paciente).
                int tiempoConsulta = rnd.Next(5, 16); // Duración de consulta aleatoria entre 5 y 15 segundos.
                int id = rnd.Next(1, 101);            // ID aleatorio entre 1 y 100.
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);
                bool requiereDiagnostico = rnd.Next(0, 2) == 1; // Aleatorio: true si requiere diagnóstico.

                // Crea una nueva instancia de Paciente.
                Paciente paciente = new Paciente(id, i * 2, tiempoConsulta, prioridad, requiereDiagnostico);

                // Lanza la atención del paciente de forma asíncrona.
                Task tarea = Task.Run(async () =>
                {
                    await Task.Delay(llegada); // Espera el tiempo de llegada simulado.
                    await AtenderPaciente(paciente, ordenLlegada);
                });

                tareas.Add(tarea);
            }

            // Espera a que todas las tareas finalicen.
            await Task.WhenAll(tareas);
            Console.WriteLine("Todas las consultas y diagnósticos han finalizado.");
        }
    }
}
