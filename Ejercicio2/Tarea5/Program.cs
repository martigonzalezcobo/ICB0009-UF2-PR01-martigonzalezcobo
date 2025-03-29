using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Enumera los estados posibles de un paciente durante el proceso de atención.
    public enum EstadoPaciente
    {
        EsperaConsulta,    // El paciente llega y espera ser atendido.
        Consulta,          // El paciente se encuentra en consulta médica.
        EsperaDiagnostico, // El paciente terminó la consulta y espera el diagnóstico.
        Finalizado         // El proceso de atención del paciente ha finalizado.
    }

    // Enumera los tipos de prioridad para clasificar la urgencia de atención.
    public enum PrioridadPaciente
    {
        Emergencia,      // Atención inmediata.
        Urgencia,        // Atención rápida pero no tan inmediata como Emergencia.
        ConsultaGeneral  // Atención programada o de menor prioridad.
    }

    // Clase que modela a un paciente y almacena sus datos para la simulación.
    public class Paciente
    {
        public int Id { get; set; }                      // Identificador único del paciente.
        public int LlegadaHospital { get; set; }         // Tiempo simulado de llegada al hospital.
        public int TiempoConsulta { get; set; }          // Duración en segundos de la consulta médica.
        public EstadoPaciente Estado { get; set; }       // Estado actual del paciente.
        public int NivelPrioridad { get; set; }          // Valor numérico derivado de la prioridad (1 = mayor, 3 = menor).
        public bool RequiereDiagnostico { get; set; }      // Indica si, además de la consulta, se requiere un diagnóstico.
        public int OrdenLlegada { get; set; }            // Orden secuencial de llegada al hospital.
        public DateTime HoraLlegada { get; set; }          // Momento real en que el paciente llega.
        public DateTime HoraInicioConsulta { get; set; }   // Momento en que el paciente inicia la consulta.

        // Constructor que inicializa las propiedades del paciente.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico, int ordenLlegada)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta;  // Estado inicial cuando el paciente llega.
            // Se asigna un nivel numérico basado en la prioridad (por ejemplo, Emergencia = 1, Urgencia = 2, ConsultaGeneral = 3).
            NivelPrioridad = (int)prioridad + 1;
            RequiereDiagnostico = requiereDiagnostico;
            OrdenLlegada = ordenLlegada;
        }
    }

    class Program
    {
        // Semáforo que limita las consultas médicas simultáneas a 4.
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);
        // Semáforo que limita el uso simultáneo de máquinas de diagnóstico a 2.
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);
        
        // Cola de espera para ordenar a los pacientes según su prioridad y orden de llegada.
        static List<Paciente> colaPacientes = new List<Paciente>();
        // Objeto para sincronizar el acceso a la cola.
        static object lockCola = new object();

        // Variables para controlar el orden de diagnóstico.
        static int pacienteEnTurno = 1;      // Mantiene el orden secuencial para el diagnóstico.
        static object bloqueoOrden = new object(); // Objeto de bloqueo para sincronizar la secuencia.

        // Variables para recolectar estadísticas de la simulación.
        static int totalEmergencias = 0, totalUrgencias = 0, totalConsultas = 0;
        // Diccionario que almacena los tiempos de espera según el nivel de prioridad.
        static Dictionary<int, List<double>> tiemposEspera = new Dictionary<int, List<double>>()
        {
            { 1, new List<double>() },
            { 2, new List<double>() },
            { 3, new List<double>() }
        };
        // Acumula el tiempo total de diagnóstico (en segundos) para calcular el uso de máquinas.
        static double tiempoTotalDiagnostico = 0;

        // Método que verifica si el paciente es el primero en la cola de espera.
        // Se ordena la cola por nivel de prioridad y, en caso de empate, por orden de llegada.
        static bool EsPrimerPaciente(Paciente paciente)
        {
            var primerPaciente = colaPacientes
                .OrderBy(p => p.NivelPrioridad)
                .ThenBy(p => p.OrdenLlegada)
                .FirstOrDefault();
            return primerPaciente != null && primerPaciente == paciente;
        }

        // Método que simula el proceso de diagnóstico para un paciente.
        // Se espera que los pacientes sean atendidos en el orden correcto usando un bloqueo.
        static async Task RealizarDiagnostico(Paciente paciente)
        {
            // Se utiliza un bloqueo para esperar el turno de diagnóstico.
            lock (bloqueoOrden)
            {
                // Si el paciente no tiene el turno (según OrdenLlegada) se espera.
                while (paciente.OrdenLlegada != pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Inicia el diagnóstico y se informa en consola.
            Console.WriteLine($"Paciente {paciente.Id} inicia diagnóstico.");
            // Se simula un tiempo de diagnóstico de 15 segundos.
            await Task.Delay(15000);
            Console.WriteLine($"Paciente {paciente.Id} finaliza diagnóstico.");

            // Se acumula el tiempo de diagnóstico para estadísticas.
            tiempoTotalDiagnostico += 15;

            // Se actualiza el turno de diagnóstico y se notifica a los hilos en espera.
            lock (bloqueoOrden)
            {
                pacienteEnTurno++;
                Monitor.PulseAll(bloqueoOrden);
            }
        }

        // Método que simula la atención completa de un paciente:
        // - Espera en la cola
        // - Realiza la consulta médica
        // - Realiza el diagnóstico si es requerido
        static async Task AtenderPaciente(Paciente paciente, int llegadaDelay)
        {
            // Simula el retraso en la llegada del paciente.
            await Task.Delay(llegadaDelay);
            // Registra la hora real de llegada.
            paciente.HoraLlegada = DateTime.Now;
            Console.WriteLine($"Paciente {paciente.Id} llega al hospital.");

            // Manejo de la cola de espera: se agrega el paciente y espera hasta ser el primero.
            lock (lockCola)
            {
                colaPacientes.Add(paciente);
                // El paciente espera en la cola hasta que sea el primero (según prioridad y orden).
                while (!EsPrimerPaciente(paciente))
                {
                    Monitor.Wait(lockCola);
                }
                // Una vez que es el primero, se elimina de la cola y se notifica a los demás.
                colaPacientes.Remove(paciente);
                Monitor.PulseAll(lockCola);
            }

            // Registra la hora en que inicia la consulta.
            paciente.HoraInicioConsulta = DateTime.Now;
            // Calcula el tiempo de espera en segundos.
            double espera = (paciente.HoraInicioConsulta - paciente.HoraLlegada).TotalSeconds;
            tiemposEspera[paciente.NivelPrioridad].Add(espera);

            // Se contabiliza el paciente en función de su tipo de atención, usando un switch.
            switch (paciente.NivelPrioridad)
            {
                case 1:
                    Interlocked.Increment(ref totalEmergencias);
                    break;
                case 2:
                    Interlocked.Increment(ref totalUrgencias);
                    break;
                case 3:
                    Interlocked.Increment(ref totalConsultas);
                    break;
            }

            // Se adquiere un recurso para la consulta médica (máximo 4 consultas simultáneas).
            await consultasMedicas.WaitAsync();
            try
            {
                // Actualiza el estado del paciente y simula la duración de la consulta.
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta.");
                await Task.Delay(paciente.TiempoConsulta * 1000); // Conversión de segundos a milisegundos.
            }
            finally
            {
                // Libera el recurso de consulta médica para que otro paciente pueda iniciar.
                consultasMedicas.Release();
            }

            // Si el paciente requiere diagnóstico adicional, se gestiona ese proceso.
            if (paciente.RequiereDiagnostico)
            {
                // Actualiza el estado a EsperaDiagnostico y notifica la transición.
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id} pasa a diagnóstico.");

                // Se adquiere el recurso de diagnóstico (máximo 2 diagnósticos simultáneos).
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    // Se llama al método que simula el diagnóstico.
                    await RealizarDiagnostico(paciente);
                }
                finally
                {
                    // Libera la máquina de diagnóstico.
                    maquinasDiagnostico.Release();
                }
            }

            // Una vez finalizados consulta y diagnóstico (si lo hubo), se actualiza el estado a Finalizado.
            paciente.Estado = EstadoPaciente.Finalizado;
            // Se calcula la duración total sumando el tiempo de consulta y, de ser necesario, los 15 segundos de diagnóstico.
            int duracionTotal = paciente.TiempoConsulta + (paciente.RequiereDiagnostico ? 15 : 0);
            Console.WriteLine($"Paciente {paciente.Id} finalizado. Duración total: {duracionTotal} segundos.");
        }

        // Punto de entrada de la aplicación.
        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 20;
            Random rnd = new Random();

            // Se registra el inicio de la simulación para calcular estadísticas finales.
            DateTime inicioSimulacion = DateTime.Now;

            // Genera pacientes aleatoriamente con retrasos escalonados.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;
                int llegadaDelay = i * 2000; // Cada paciente llega con un retraso de 2 segundos.
                int tiempoConsulta = rnd.Next(5, 16); // Duración aleatoria de la consulta entre 5 y 15 segundos.
                int id = rnd.Next(1, 101); // ID aleatorio entre 1 y 100.
                // Selecciona aleatoriamente la prioridad.
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);
                // Determina de forma aleatoria si se requiere diagnóstico.
                bool requiereDiagnostico = rnd.Next(0, 2) == 1;

                // Se crea una nueva instancia de paciente con sus parámetros.
                Paciente paciente = new Paciente(id, i * 2, tiempoConsulta, prioridad, requiereDiagnostico, ordenLlegada);
                // Se lanza la tarea de atención para el paciente.
                Task tarea = Task.Run(() => AtenderPaciente(paciente, llegadaDelay));
                tareas.Add(tarea);
            }

            // Espera a que todas las tareas finalicen.
            await Task.WhenAll(tareas);
            DateTime finSimulacion = DateTime.Now;

            // Se calcula la duración total de la simulación en segundos.
            double duracionSimulacion = (finSimulacion - inicioSimulacion).TotalSeconds;

            // Cálculo del porcentaje de uso de las máquinas de diagnóstico.
            double usoDiagnostico = (tiempoTotalDiagnostico / (2 * duracionSimulacion)) * 100;

            // Cálculo de los tiempos de espera promedio por prioridad.
            double promEmergencia = tiemposEspera[1].Any() ? tiemposEspera[1].Average() : 0;
            double promUrgencia = tiemposEspera[2].Any() ? tiemposEspera[2].Average() : 0;
            double promConsulta = tiemposEspera[3].Any() ? tiemposEspera[3].Average() : 0;

            // Se imprime un resumen final de la simulación.
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
