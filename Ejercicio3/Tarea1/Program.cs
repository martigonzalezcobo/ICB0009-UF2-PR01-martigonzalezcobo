using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Define los posibles estados que puede tener un paciente durante el proceso de atención.
    public enum EstadoPaciente
    {
        EsperaConsulta,    // El paciente llega y espera ser atendido.
        Consulta,          // El paciente está en consulta médica.
        EsperaDiagnostico, // El paciente terminó la consulta y espera el diagnóstico.
        Finalizado         // El proceso de atención ha concluido.
    }

    // Define las prioridades de atención, de mayor a menor urgencia.
    public enum PrioridadPaciente
    {
        Emergencia,      // Atención inmediata.
        Urgencia,        // Atención rápida.
        ConsultaGeneral  // Atención programada o de menor urgencia.
    }

    // Clase que representa a un paciente y contiene la información necesaria para la simulación.
    public class Paciente
    {
        public int Id { get; set; }                      // Identificador único del paciente.
        public int LlegadaHospital { get; set; }         // Tiempo simulado de llegada al hospital (en segundos o unidades).
        public int TiempoConsulta { get; set; }          // Duración de la consulta médica en segundos.
        public EstadoPaciente Estado { get; set; }       // Estado actual del paciente en el flujo de atención.
        public int NivelPrioridad { get; set; }          // Valor numérico asignado según la prioridad (1 = mayor, 3 = menor).
        public bool RequiereDiagnostico { get; set; }      // Indica si el paciente requiere un diagnóstico posterior.
        public int OrdenLlegada { get; set; }            // Orden secuencial en el que el paciente llega.
        public DateTime HoraLlegada { get; set; }          // Momento real en que el paciente llega al hospital.
        public DateTime HoraInicioConsulta { get; set; }   // Momento en que inicia la consulta médica.

        // Constructor: inicializa las propiedades básicas del paciente.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico, int ordenLlegada)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta;  // Al crear al paciente, se asume que espera consulta.
            // Se asigna un valor numérico a la prioridad (por ejemplo, Emergencia = 1, Urgencia = 2, ConsultaGeneral = 3).
            NivelPrioridad = (int)prioridad + 1;
            RequiereDiagnostico = requiereDiagnostico;
            OrdenLlegada = ordenLlegada;
        }
    }

    class Program
    {
        // Semáforo que limita a 4 el número de consultas médicas que se pueden realizar simultáneamente.
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);
        // Semáforo que limita a 2 el número de diagnósticos simultáneos (máquinas de diagnóstico disponibles).
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);

        // Lista que actúa como cola de espera para ordenar a los pacientes según su prioridad y orden de llegada.
        static List<Paciente> colaPacientes = new List<Paciente>();
        // Objeto para proteger el acceso a la cola (para evitar condiciones de carrera).
        static object lockCola = new object();

        // Variables para gestionar el orden secuencial de diagnóstico.
        static int pacienteEnTurno = 1;      // Número que indica el turno actual para el diagnóstico.
        static object bloqueoOrden = new object(); // Objeto de bloqueo para sincronizar la secuencia de diagnóstico.

        // Variables para recopilar estadísticas de la simulación.
        static int totalEmergencias = 0, totalUrgencias = 0, totalConsultas = 0;
        // Diccionario que almacena listas de tiempos de espera para cada nivel de prioridad.
        static Dictionary<int, List<double>> tiemposEspera = new Dictionary<int, List<double>>()
        {
            { 1, new List<double>() },
            { 2, new List<double>() },
            { 3, new List<double>() }
        };
        // Variable para acumular el tiempo total de diagnóstico, útil para calcular el uso de las máquinas.
        static double tiempoTotalDiagnostico = 0;

        // Método que determina si el paciente es el primero en la cola de espera.
        // La cola se ordena primero por NivelPrioridad y luego por OrdenLlegada.
        static bool EsPrimerPaciente(Paciente paciente)
        {
            var primerPaciente = colaPacientes
                .OrderBy(p => p.NivelPrioridad)
                .ThenBy(p => p.OrdenLlegada)
                .FirstOrDefault();
            // Retorna verdadero si el paciente actual es el primero en la cola.
            return primerPaciente != null && primerPaciente == paciente;
        }

        // Método que simula el proceso de diagnóstico para un paciente.
        // Se espera que los pacientes sean diagnosticados en el orden correcto.
        static async Task RealizarDiagnostico(Paciente paciente)
        {
            // Se utiliza un bloqueo para asegurarse de que el paciente tenga el turno de diagnóstico.
            lock (bloqueoOrden)
            {
                // Mientras el OrdenLlegada del paciente no sea igual al turno actual, se espera.
                while (paciente.OrdenLlegada != pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Una vez que tiene el turno, se inicia el diagnóstico.
            Console.WriteLine($"Paciente {paciente.Id} inicia diagnóstico.");
            // Se simula el proceso de diagnóstico con una demora de 15 segundos.
            await Task.Delay(15000);
            Console.WriteLine($"Paciente {paciente.Id} finaliza diagnóstico.");

            // Se suma el tiempo de diagnóstico para futuras estadísticas.
            tiempoTotalDiagnostico += 15;

            // Se actualiza el turno para el siguiente paciente y se notifica a los hilos en espera.
            lock (bloqueoOrden)
            {
                pacienteEnTurno++;
                Monitor.PulseAll(bloqueoOrden);
            }
        }

        // Método que simula la atención completa de un paciente:
        // 1. Espera en la cola de pacientes (según prioridad y orden).
        // 2. Realiza la consulta médica.
        // 3. Si es necesario, pasa al diagnóstico.
        static async Task AtenderPaciente(Paciente paciente, int llegadaDelay)
        {
            // Se simula el retraso en la llegada del paciente.
            await Task.Delay(llegadaDelay);
            // Se registra la hora real de llegada.
            paciente.HoraLlegada = DateTime.Now;
            Console.WriteLine($"Paciente {paciente.Id} llega al hospital.");

            // Se maneja la cola de espera de pacientes con bloqueo para evitar conflictos.
            lock (lockCola)
            {
                colaPacientes.Add(paciente);
                // El paciente espera en la cola hasta ser el primero, según el orden definido.
                while (!EsPrimerPaciente(paciente))
                {
                    Monitor.Wait(lockCola);
                }
                // Una vez que es el primero, se elimina de la cola y se notifica a los demás.
                colaPacientes.Remove(paciente);
                Monitor.PulseAll(lockCola);
            }

            // Se registra el momento en que inicia la consulta y se calcula el tiempo de espera.
            paciente.HoraInicioConsulta = DateTime.Now;
            double espera = (paciente.HoraInicioConsulta - paciente.HoraLlegada).TotalSeconds;
            tiemposEspera[paciente.NivelPrioridad].Add(espera);

            // Se adquiere el recurso para iniciar la consulta médica (máximo 4 consultas simultáneas).
            await consultasMedicas.WaitAsync();
            try
            {
                // Se actualiza el estado a "Consulta" y se simula el tiempo de la consulta.
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta.");
                await Task.Delay(paciente.TiempoConsulta * 1000); // Tiempo de consulta en milisegundos.
            }
            finally
            {
                // Se libera el recurso de consulta para que otro paciente pueda iniciar.
                consultasMedicas.Release();
            }

            // Si el paciente requiere diagnóstico adicional, se procede a esa etapa.
            if (paciente.RequiereDiagnostico)
            {
                // Se actualiza el estado a "EsperaDiagnostico" y se notifica la transición.
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id} pasa a diagnóstico.");
                // Se adquiere una máquina de diagnóstico (máximo 2 diagnósticos simultáneos).
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    // Se ejecuta el método que simula el diagnóstico.
                    await RealizarDiagnostico(paciente);
                }
                finally
                {
                    // Se libera la máquina de diagnóstico para otros pacientes.
                    maquinasDiagnostico.Release();
                }
            }

            // Al finalizar consulta (y diagnóstico, si corresponde), se actualiza el estado a "Finalizado".
            paciente.Estado = EstadoPaciente.Finalizado;
            Console.WriteLine($"Paciente {paciente.Id} finalizado.");
        }

        // Método que genera pacientes de manera continua hasta alcanzar el número especificado.
        // Cada paciente se crea con parámetros aleatorios y se lanza la atención de forma asíncrona.
        static async Task GeneradorPacientes(int numeroPacientes)
        {
            int contador = 0;
            Random rnd = new Random();
            while (contador < numeroPacientes)
            {
                contador++;
                int ordenLlegada = contador; // Se asigna el orden según el contador.
                int tiempoConsulta = rnd.Next(5, 16); // Tiempo de consulta aleatorio entre 5 y 15 segundos.
                int id = rnd.Next(1, 101); // ID aleatorio para el paciente.
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3); // Selecciona una prioridad aleatoria.
                bool requiereDiagnostico = rnd.Next(0, 2) == 1; // Determina aleatoriamente si se requiere diagnóstico.
                
                // Crea una nueva instancia de paciente con los parámetros generados.
                Paciente paciente = new Paciente(id, contador * 2, tiempoConsulta, prioridad, requiereDiagnostico, ordenLlegada);
                // Se lanza la tarea de atención para el paciente sin retraso adicional (ya se gestiona dentro de AtenderPaciente).
                _ = Task.Run(() => AtenderPaciente(paciente, 0));

                // Se espera 2 segundos antes de generar el siguiente paciente.
                await Task.Delay(2000);
            }
        }

        // Punto de entrada principal de la aplicación.
        static async Task Main(string[] args)
        {
            int numeroPacientes = 50; // Define el número total de pacientes a generar.
            DateTime inicioSimulacion = DateTime.Now; // Se registra el inicio de la simulación.

            // Se inician las tareas de generación de pacientes.
            await GeneradorPacientes(numeroPacientes);
            // Se espera un tiempo adicional para asegurar que todas las tareas hayan finalizado.
            await Task.Delay(30000);

            DateTime finSimulacion = DateTime.Now;
            // Se calcula la duración total de la simulación en segundos.
            double duracionSimulacion = (finSimulacion - inicioSimulacion).TotalSeconds;
            // Se calcula el uso promedio de las máquinas de diagnóstico basado en el tiempo total acumulado.
            double usoDiagnostico = (tiempoTotalDiagnostico / (2 * duracionSimulacion)) * 100;

            // Se imprime un resumen final de la simulación.
            Console.WriteLine("\n--- FIN DEL DÍA ---");
            Console.WriteLine($"Uso promedio de máquinas de diagnóstico: {usoDiagnostico:F1}%");
        }
    }
}
