using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Enumeración que define los estados del paciente.
    public enum EstadoPaciente
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Finalizado
    }

    // Enumeración para establecer la prioridad de atención.
    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

    // Clase que representa al paciente.
    public class Paciente
    {
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public PrioridadPaciente Prioridad { get; set; }
        public bool RequiereDiagnostico { get; set; } // Indica si se requiere diagnóstico.

        // Constructor que inicializa el paciente con estado de espera.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta;  // Estado inicial.
            Prioridad = prioridad;
            RequiereDiagnostico = requiereDiagnostico;
        }
    }

    class Program
    {
        // Limita el número de diagnósticos concurrentes a 2.
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);

        // Variables y objeto de bloqueo para gestionar el orden de diagnóstico.
        static int pacienteEnTurno = 1;
        static object bloqueoOrden = new object();

        // Método que realiza el diagnóstico respetando el orden de llegada.
        static async Task RealizarDiagnostico(Paciente paciente, int ordenLlegada)
        {
            lock (bloqueoOrden)
            {
                // Espera activamente hasta que el paciente tenga su turno.
                while (ordenLlegada != pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Inicio del diagnóstico.
            Console.WriteLine($"Paciente {paciente.Id} con orden {ordenLlegada} inicia diagnóstico.");
            await Task.Delay(15000); // Simula 15 segundos de diagnóstico.
            Console.WriteLine($"Paciente {paciente.Id} con orden {ordenLlegada} finaliza diagnóstico.");

            lock (bloqueoOrden)
            {
                pacienteEnTurno++; // Avanza al siguiente turno.
                Monitor.PulseAll(bloqueoOrden); // Notifica a los pacientes en espera.
            }
        }

        // Simula la atención completa del paciente.
        static async Task AtenderPaciente(Paciente paciente, int ordenLlegada)
        {
            // Muestra la información inicial del paciente.
            Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: {paciente.Estado}. Prioridad: {paciente.Prioridad}.");

            paciente.Estado = EstadoPaciente.Consulta;
            Console.WriteLine($"Paciente {paciente.Id}. Entra en consulta.");
            
            // Simula el tiempo de consulta.
            await Task.Delay(paciente.TiempoConsulta * 1000);

            if (paciente.RequiereDiagnostico)
            {
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id} requiere diagnóstico.");

                // Se adquiere una máquina de diagnóstico.
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    // Realiza el diagnóstico respetando el orden.
                    await RealizarDiagnostico(paciente, ordenLlegada);
                }
                finally
                {
                    // Libera la máquina de diagnóstico.
                    maquinasDiagnostico.Release();
                }
            }
            paciente.Estado = EstadoPaciente.Finalizado;
            // Imprime el tiempo total de atención, sumando consulta y diagnóstico si se realizó.
            Console.WriteLine($"Paciente {paciente.Id} finalizado. Duración {(paciente.RequiereDiagnostico ? "Diagnóstico + Consulta" : "Consulta")}: {paciente.TiempoConsulta + (paciente.RequiereDiagnostico ? 15 : 0)} segundos.");
        }

        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4; // Define el número de pacientes a simular.
            Random rnd = new Random();

            // Crea tareas para cada paciente con llegada escalonada.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;
                int llegada = i * 2000;  // Retraso de llegada de 2 segundos por paciente.
                int tiempoConsulta = rnd.Next(5, 16);
                int id = rnd.Next(1, 101);
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);
                bool requiereDiagnostico = rnd.Next(0, 2) == 1;

                Paciente paciente = new Paciente(id, i * 2, tiempoConsulta, prioridad, requiereDiagnostico);

                // Lanza cada tarea de atención tras el retraso simulado.
                Task tarea = Task.Run(async () =>
                {
                    await Task.Delay(llegada);
                    await AtenderPaciente(paciente, ordenLlegada);
                });

                tareas.Add(tarea);
            }

            // Espera a que se completen todas las tareas.
            await Task.WhenAll(tareas);
            Console.WriteLine("Todas las consultas y diagnósticos han finalizado.");
        }
    }
}
