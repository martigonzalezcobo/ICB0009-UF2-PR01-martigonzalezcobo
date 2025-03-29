using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Estados posibles de atención para un paciente.
    public enum EstadoPaciente
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Finalizado
    }

    // Prioridades definidas para los pacientes.
    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

    // Clase que representa a un paciente, iniciando en "EsperaConsulta".
    public class Paciente
    {
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public PrioridadPaciente Prioridad { get; set; }
        public bool RequiereDiagnostico { get; set; } // Indica si se necesita diagnóstico.

        // Constructor que inicializa las propiedades del paciente.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta; // Estado inicial.
            Prioridad = prioridad;
            RequiereDiagnostico = requiereDiagnostico;
        }
    }

    class Program
    {
        // Limita el número de diagnósticos simultáneos a 2.
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);
        // Limita el número de consultas simultáneas a 4.
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);

        // Variables para controlar el orden de diagnóstico.
        static int pacienteEnTurno = 1;
        static object bloqueoOrden = new object();

        // Método que realiza el diagnóstico en orden utilizando bloqueo y Monitor.
        static async Task RealizarDiagnostico(Paciente paciente, int ordenLlegada)
        {
            lock (bloqueoOrden)
            {
                // Espera hasta que el paciente tenga el turno correcto.
                while (ordenLlegada != pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Inicia el diagnóstico.
            Console.WriteLine($"Paciente {paciente.Id} con orden {ordenLlegada} inicia diagnóstico.");
            await Task.Delay(15000); // Simula 15 segundos de diagnóstico.
            Console.WriteLine($"Paciente {paciente.Id} con orden {ordenLlegada} finaliza diagnóstico.");

            lock (bloqueoOrden)
            {
                pacienteEnTurno++; // Se incrementa el turno para el siguiente paciente.
                Monitor.PulseAll(bloqueoOrden); // Notifica a los pacientes en espera.
            }
        }

        // Método que simula la atención completa: consulta y, si es necesario, diagnóstico.
        static async Task AtenderPaciente(Paciente paciente, int ordenLlegada)
        {
            // Muestra los datos iniciales del paciente al llegar.
            Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: {paciente.Estado}. Prioridad: {paciente.Prioridad}.");

            // Se adquiere un recurso de consulta disponible.
            await consultasMedicas.WaitAsync();
            try
            {
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta. (Orden de llegada: {ordenLlegada})");
                // Simula la duración de la consulta.
                await Task.Delay(paciente.TiempoConsulta * 1000);
            }
            finally
            {
                // Libera el recurso de consulta.
                consultasMedicas.Release();
            }

            // Si se requiere diagnóstico, se procede a solicitar el recurso.
            if (paciente.RequiereDiagnostico)
            {
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id} termina consulta y pasa a {paciente.Estado}. Prioridad: {paciente.Prioridad}. Duración Consulta: {paciente.TiempoConsulta} segundos.");

                await maquinasDiagnostico.WaitAsync();
                try
                {
                    // Realiza el diagnóstico respetando el orden de llegada.
                    await RealizarDiagnostico(paciente, ordenLlegada);
                }
                finally
                {
                    // Libera la máquina de diagnóstico.
                    maquinasDiagnostico.Release();
                }
            }

            paciente.Estado = EstadoPaciente.Finalizado;
            // Muestra la duración total de atención (consulta más diagnóstico, si se realizó).
            Console.WriteLine($"Paciente {paciente.Id}. Finalizado. Orden: {ordenLlegada}. Prioridad: {paciente.Prioridad}. Duración total: {paciente.TiempoConsulta + (paciente.RequiereDiagnostico ? 15 : 0)} segundos.");
        }

        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 20; // Simula 20 pacientes.
            Random rnd = new Random();

            // Genera tareas para simular la llegada escalonada y atención de cada paciente.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;
                int llegada = i * 2000;  // Cada paciente llega con un retraso de 2 segundos.
                int tiempoConsulta = rnd.Next(5, 16);
                int id = rnd.Next(1, 101);
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);
                bool requiereDiagnostico = rnd.Next(0, 2) == 1;

                Paciente paciente = new Paciente(id, i * 2, tiempoConsulta, prioridad, requiereDiagnostico);

                Task tarea = Task.Run(async () =>
                {
                    await Task.Delay(llegada); // Simula el tiempo de llegada del paciente.
                    await AtenderPaciente(paciente, ordenLlegada);
                });

                tareas.Add(tarea);
            }

            // Espera a que se completen todas las tareas de atención.
            await Task.WhenAll(tareas);
            Console.WriteLine("Todas las consultas y diagnósticos han finalizado.");
        }
    }
}
