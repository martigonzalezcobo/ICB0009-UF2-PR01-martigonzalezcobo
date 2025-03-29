using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    // Enumeración que define los diferentes estados por los que puede pasar un paciente.
    public enum EstadoPaciente
    {
        EsperaConsulta,    // Estado inicial: el paciente espera ser atendido.
        Consulta,          // El paciente está en consulta médica.
        EsperaDiagnostico, // Finalizada la consulta, el paciente espera el diagnóstico.
        Finalizado         // El paciente ha completado todo el proceso.
    }

    // Enumeración que define los tipos de prioridad para clasificar a los pacientes.
    public enum PrioridadPaciente
    {
        Emergencia,      // Máxima prioridad, atención inmediata.
        Urgencia,        // Prioridad intermedia.
        ConsultaGeneral  // Menor prioridad en comparación con los anteriores.
    }

    // Clase que representa a un paciente y almacena sus datos relevantes para la simulación.
    public class Paciente
    {
        public int Id { get; set; }                      // Identificador único del paciente.
        public int LlegadaHospital { get; set; }         // Tiempo (simulado) en que llega al hospital.
        public int TiempoConsulta { get; set; }          // Duración de la consulta en segundos.
        public EstadoPaciente Estado { get; set; }       // Estado actual del paciente.
        public int NivelPrioridad { get; set; }          // Nivel numérico de prioridad (1, 2 o 3).
        public bool RequiereDiagnostico { get; set; }      // Indica si el paciente necesita un diagnóstico adicional.
        public int OrdenLlegada { get; set; }            // Orden secuencial en que el paciente llega.

        // Constructor que inicializa las propiedades del paciente.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad, bool requiereDiagnostico, int ordenLlegada)
        {
            Id = id;
            LlegadaHospital = llegadaHospital;
            TiempoConsulta = tiempoConsulta;
            Estado = EstadoPaciente.EsperaConsulta;  // Estado inicial al llegar.
            // Se asigna un nivel numérico a la prioridad (por ejemplo, Emergencia = 1, Urgencia = 2, ConsultaGeneral = 3).
            NivelPrioridad = (int)prioridad + 1;
            RequiereDiagnostico = requiereDiagnostico;
            OrdenLlegada = ordenLlegada;
        }
    }

    class Program
    {
        // Semáforo para limitar las consultas médicas concurrentes a 4.
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);
        // Semáforo para limitar las máquinas de diagnóstico a 2.
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);
        // Lista que actúa como cola de espera para ordenar pacientes según prioridad y orden de llegada.
        static List<Paciente> colaPacientes = new List<Paciente>();
        // Objeto para sincronizar el acceso a la cola.
        static object lockCola = new object();

        // Variables para sincronizar el orden de diagnóstico.
        static int pacienteEnTurno = 1;      // Indica cuál es el orden de turno actual.
        static object bloqueoOrden = new object(); // Objeto para bloqueo de la secuencia de diagnóstico.

        // Método que verifica si el paciente es el siguiente a ser atendido.
        // Se ordena la cola primero por NivelPrioridad y luego por OrdenLlegada.
        static bool EsPrimerPaciente(Paciente paciente)
        {
            var primerPaciente = colaPacientes
                                 .OrderBy(p => p.NivelPrioridad)
                                 .ThenBy(p => p.OrdenLlegada)
                                 .FirstOrDefault();
            return primerPaciente != null && primerPaciente == paciente;
        }

        // Método asíncrono que simula el proceso de diagnóstico de un paciente.
        // Se respeta el orden de llegada mediante el bloqueo (Monitor).
        static async Task RealizarDiagnostico(Paciente paciente)
        {
            // Bloqueo para esperar hasta que el paciente tenga el turno correspondiente.
            lock (bloqueoOrden)
            {
                // Si el OrdenLlegada del paciente no coincide con el turno actual, se espera.
                while (paciente.OrdenLlegada != pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            // Inicio del diagnóstico, se informa en consola.
            Console.WriteLine($"Paciente {paciente.Id} (Orden {paciente.OrdenLlegada}, Prioridad {paciente.NivelPrioridad}) inicia diagnóstico.");
            // Se simula el proceso de diagnóstico durante 15 segundos.
            await Task.Delay(15000);
            Console.WriteLine($"Paciente {paciente.Id} (Orden {paciente.OrdenLlegada}, Prioridad {paciente.NivelPrioridad}) finaliza diagnóstico.");

            // Una vez finalizado, se actualiza el turno y se notifica a otros hilos que esperan.
            lock (bloqueoOrden)
            {
                pacienteEnTurno++;
                Monitor.PulseAll(bloqueoOrden);
            }
        }

        // Método asíncrono que simula la atención completa del paciente,
        // desde su llegada, la consulta médica y el diagnóstico si es necesario.
        static async Task AtenderPaciente(Paciente paciente, int llegadaDelay)
        {
            // Simula el retraso en la llegada del paciente.
            await Task.Delay(llegadaDelay);
            Console.WriteLine($"Paciente {paciente.Id} llega al hospital. Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad}");

            // Manejo de la cola de espera: se agrega el paciente y se espera hasta ser el primero en la cola.
            lock (lockCola)
            {
                colaPacientes.Add(paciente);
                // Mientras el paciente no sea el primero según la cola (prioridad y orden), se espera.
                while (!EsPrimerPaciente(paciente))
                {
                    Monitor.Wait(lockCola);
                }
                // Una vez que es el primero, se retira de la cola y se notifica a otros pacientes.
                colaPacientes.Remove(paciente);
                Monitor.PulseAll(lockCola);
            }

            // Se adquiere un recurso para la consulta médica.
            await consultasMedicas.WaitAsync();
            try
            {
                // Actualiza el estado a "Consulta" y simula la duración de la consulta.
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta. (Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad})");
                await Task.Delay(paciente.TiempoConsulta * 1000);
            }
            finally
            {
                // Libera el recurso de consulta para que otro paciente pueda utilizarlo.
                consultasMedicas.Release();
            }

            // Si el paciente requiere diagnóstico, se procede a gestionar esta etapa.
            if (paciente.RequiereDiagnostico)
            {
                // Cambia el estado a "EsperaDiagnostico" e informa.
                paciente.Estado = EstadoPaciente.EsperaDiagnostico;
                Console.WriteLine($"Paciente {paciente.Id} termina consulta y pasa a {paciente.Estado} (Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad}). Duración Consulta: {paciente.TiempoConsulta} segundos.");

                // Se espera a que haya una máquina de diagnóstico disponible.
                await maquinasDiagnostico.WaitAsync();
                try
                {
                    // Realiza el diagnóstico respetando el orden.
                    await RealizarDiagnostico(paciente);
                }
                finally
                {
                    // Libera la máquina de diagnóstico para otros pacientes.
                    maquinasDiagnostico.Release();
                }
            }

            // Una vez completadas consulta y diagnóstico, se marca al paciente como finalizado.
            paciente.Estado = EstadoPaciente.Finalizado;
            // Calcula la duración total: tiempo de consulta + 15 segundos adicionales si hubo diagnóstico.
            int duracionTotal = paciente.TiempoConsulta + (paciente.RequiereDiagnostico ? 15 : 0);
            Console.WriteLine($"Paciente {paciente.Id} finalizado. Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad}. Duración total: {duracionTotal} segundos.");
        }

        // Punto de entrada principal: genera y atiende a los pacientes simulando su llegada escalonada.
        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 20;  // Número total de pacientes a simular.
            Random rnd = new Random();

            // Genera los pacientes con características aleatorias y un retraso escalonado en su llegada.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;           // Se asigna un orden de llegada secuencial.
                int llegadaDelay = i * 2000;          // Cada paciente llega 2 segundos después del anterior.
                int tiempoConsulta = rnd.Next(5, 16);   // La duración de la consulta varía entre 5 y 15 segundos.
                int id = rnd.Next(1, 101);            // Se asigna un ID aleatorio entre 1 y 100.
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);  // Se asigna una prioridad aleatoria.
                bool requiereDiagnostico = rnd.Next(0, 2) == 1; // Aleatoriamente, el paciente puede requerir diagnóstico.

                // Crea la instancia del paciente con los datos generados.
                Paciente paciente = new Paciente(id, i * 2, tiempoConsulta, prioridad, requiereDiagnostico, ordenLlegada);

                // Se lanza la atención del paciente de forma asíncrona y se agrega la tarea a la lista.
                Task tarea = Task.Run(() => AtenderPaciente(paciente, llegadaDelay));
                tareas.Add(tarea);
            }

            // Espera a que se completen todas las tareas (todos los pacientes han sido atendidos).
            await Task.WhenAll(tareas);
            Console.WriteLine("Todas las consultas y diagnósticos han finalizado.");
        }
    }
}
