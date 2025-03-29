// Importación de namespaces necesarios
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Namespace principal del proyecto
namespace GestionAtencionHospitalaria
{
    // Enumeración para estados de paciente
    public enum EstadoPaciente
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Finalizado
    }

    // Enumeración para prioridad de pacientes
    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

    // Clase que representa a un paciente
    public class Paciente
    {
        // Propiedades del paciente
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public int NivelPrioridad { get; set; }
        public bool RequiereDiagnostico { get; set; }
        public int OrdenLlegada { get; set; }
        public DateTime HoraLlegada { get; set; }
        public DateTime HoraInicioConsulta { get; set; }

        // Constructor de la clase Paciente
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico, int ordenLlegada)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta;
            NivelPrioridad = (int)prioridad + 1;
            RequiereDiagnostico = requiereDiagnostico;
            OrdenLlegada = ordenLlegada;
        }
    }

    class Program
    {
        // Semáforos para controlar el acceso a recursos (consultas médicas y máquinas de diagnóstico)
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);

        // Cola de pacientes y objeto para sincronización
        static List<Paciente> colaPacientes = new List<Paciente>();
        static object lockCola = new object();

        // Variables para controlar el orden de atención y estadísticas
        static int pacienteEnTurno = 1;
        static object bloqueoOrden = new object();
        static int totalEmergencias = 0, totalUrgencias = 0, totalConsultas = 0;
        static Dictionary<int, List<double>> tiemposEspera = new Dictionary<int, List<double>>() {... };
        static double tiempoTotalDiagnostico = 0;

        // Método para determinar si un paciente es el primero en la cola
        static bool EsPrimerPaciente(Paciente paciente)
        {
            // Ordena la cola por prioridad y orden de llegada, y selecciona el primero
            var primerPaciente = colaPacientes.OrderBy(p => p.NivelPrioridad).ThenBy(p => p.OrdenLlegada).FirstOrDefault();
            return primerPaciente!= null && primerPaciente == paciente;
        }

        // Método asincrónico para realizar el diagnóstico de un paciente
        static async Task RealizarDiagnostico(Paciente paciente)
        {
            // Espera a que sea el turno del paciente
            lock (bloqueoOrden)
            {
                while (paciente.OrdenLlegada!= pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Simula el diagnóstico (15 segundos)
            Console.WriteLine($"Paciente {paciente.Id} inicia diagnóstico.");
            await Task.Delay(15000);
            Console.WriteLine($"Paciente {paciente.Id} finaliza diagnóstico.");

            // Actualiza estadísticas
            tiempoTotalDiagnostico += 15;

            // Avanza al siguiente paciente en la cola
            lock (bloqueoOrden)
            {
                pacienteEnTurno++;
                Monitor.PulseAll(bloqueoOrden);
            }
        }

        // Método asincrónico para atender a un paciente
        static async Task AtenderPaciente(Paciente paciente, int llegadaDelay)
        {
            // Simula la llegada del paciente al hospital
            await Task.Delay(llegadaDelay);
            paciente.HoraLlegada = DateTime.Now;
            Console.WriteLine($"Paciente {paciente.Id} llega al hospital.");

            // Agrega el paciente a la cola y espera a que sea atendido
            lock (lockCola)
            {
                colaPacientes.Add(paciente);
                while (!EsPrimerPaciente(paciente))
                {
                    Monitor.Wait(lockCola);
                }
                colaPacientes.Remove(paciente);
                Monitor.PulseAll(lockCola);
            }

            // Registra el inicio de la consulta y actualiza estadísticas
            paciente.HoraInicioConsulta = DateTime.Now;
            double espera = (paciente.HoraInicioConsulta - paciente.HoraLlegada).TotalSeconds;
            tiemposEspera[paciente.NivelPrioridad].Add(espera);
            //...

            // Atiende al paciente (consulta y diagnóstico si aplica)
            await consultasMedicas.WaitAsync();
            try
            {
                // Simula la consulta
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta.");
                await Task.Delay(paciente.TiempoConsulta * 1000);
            }
            finally
            {
                consultasMedicas.Release();
            }

            if (paciente.RequiereDiagnostico)
            {
                // Simula el diagnóstico
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    await RealizarDiagnostico(paciente);
                }
                finally
                {
                    maquinasDiagnostico.Release();
                }
            }

            // Finaliza la atención del paciente
            paciente.Estado = EstadoPaciente.Finalizado;
            int duracionTotal = paciente.TiempoConsulta + (paciente.RequiereDiagnostico? 15 : 0);
            Console.WriteLine($"Paciente {paciente.Id} finalizado. Duración total: {duracionTotal} segundos.");
        }

        // Método asincrónico para generar pacientes
        static async Task GeneradorPacientes(int numeroPacientes)
        {
            // Genera pacientes con características aleatorias y los agrega a la cola
            int contador = 0;
            Random rnd = new Random();
            while (contador < numeroPacientes)
            {
                //...
                Paciente paciente = new Paciente(id, contador * 2, tiempoConsulta, prioridad, requiereDiagnostico, ordenLlegada);
                
                _ = Task.Run(() => AtenderPaciente(paciente, 0));

                Console.WriteLine($"[Generador] Paciente {paciente.Id} generado.");
                await Task.Delay(2000);
            }
        }

        // Punto de entrada del programa
        static async Task Main(string[] args)
        {
            // Configuración inicial
            int numeroPacientes = 50;
            DateTime inicioSimulacion = DateTime.Now;

            // Inicia la generación de pacientes y la simulación
            await GeneradorPacientes(numeroPacientes);
            await Task.Delay(30000);

            // Calcula y muestra estadísticas finales
            DateTime finSimulacion = DateTime.Now;
            double duracionSimulacion = (finSimulacion - inicioSimulacion).TotalSeconds;
            double usoDiagnostico = (tiempoTotalDiagnostico / (2 * duracionSimulacion)) * 100;

            // Muestra estadísticas de atención y tiempos de espera
            Console.WriteLine("\n--- FIN DEL DÍA ---");
            Console.WriteLine("Pacientes atendidos:");
            Console.WriteLine($"- Emergencias: {totalEmergencias}");
            Console.WriteLine($"- Urgencias: {totalUrgencias}");
            Console.WriteLine($"- Consultas generales: {totalConsultas}");
            Console.WriteLine("Tiempo promedio de espera:");
            Console.WriteLine($"- Emergencias: {promEmergencia:F1}s");
            Console.WriteLine($"- Urgencias: {promUrgencia:F1}s");
            Console.WriteLine($"- Consultas generales: {promConsulta:F1}s");
            Console.WriteLine($"Uso promedio de máquinas de diagnóstico: {usoDiagnostico:F1}%");
        }
    }
}
