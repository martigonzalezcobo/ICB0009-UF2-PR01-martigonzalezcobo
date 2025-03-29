using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    public enum EstadoPaciente
    {
        EsperaConsulta,
        Consulta,
        EsperaDiagnostico,
        Finalizado
    }

    public enum PrioridadPaciente
    {
        Emergencia,
        Urgencia,
        ConsultaGeneral
    }

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
            NivelPrioridad = (int)prioridad + 1; // Convertir prioridad a número (1-3)
            RequiereDiagnostico = requiereDiagnostico;
            OrdenLlegada = ordenLlegada;
        }
    }

    class Program
    {
        static SemaphoreSlim consultasMedicas = new SemaphoreSlim(4);
        static SemaphoreSlim maquinasDiagnostico = new SemaphoreSlim(2);

        // Cola de pacientes y objeto para sincronización
        static List<Paciente> colaPacientes = new List<Paciente>();
        static object lockCola = new object();

        static int pacienteEnTurno = 1;
        static object bloqueoOrden = new object();

        static int totalEmergencias = 0, totalUrgencias = 0, totalConsultas = 0;
        static Dictionary<int, List<double>> tiemposEspera = new Dictionary<int, List<double>>()
        {
            { 1, new List<double>() },
            { 2, new List<double>() },
            { 3, new List<double>() }
        };
        static double tiempoTotalDiagnostico = 0;

        static bool EsPrimerPaciente(Paciente paciente)
        {
            // Ordena la cola por prioridad y orden de llegada, y selecciona el primero
            var primerPaciente = colaPacientes.OrderBy(p => p.NivelPrioridad).ThenBy(p => p.OrdenLlegada).FirstOrDefault();
            return primerPaciente!= null && primerPaciente == paciente;
        }

        static async Task RealizarDiagnostico(Paciente paciente)
        {
            lock (bloqueoOrden)
            {
                while (paciente.OrdenLlegada!= pacienteEnTurno)
                {
                    Monitor.Wait(bloqueoOrden);
                }
            }

            Console.WriteLine($"Paciente {paciente.Id} (Orden {paciente.OrdenLlegada}, Prioridad {paciente.NivelPrioridad}) inicia diagnóstico.");
            await Task.Delay(15000);
            Console.WriteLine($"Paciente {paciente.Id} (Orden {paciente.OrdenLlegada}, Prioridad {paciente.NivelPrioridad}) finaliza diagnóstico.");

            tiempoTotalDiagnostico += 15;

            lock (bloqueoOrden)
            {
                pacienteEnTurno++;
                Monitor.PulseAll(bloqueoOrden);
            }
        }

        static async Task AtenderPaciente(Paciente paciente, int llegadaDelay)
        {
            await Task.Delay(llegadaDelay);
            paciente.HoraLlegada = DateTime.Now;
            Console.WriteLine($"Paciente {paciente.Id} llega al hospital.");

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

            paciente.HoraInicioConsulta = DateTime.Now;
            double espera = (paciente.HoraInicioConsulta - paciente.HoraLlegada).TotalSeconds;
            tiemposEspera[paciente.NivelPrioridad].Add(espera);

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

            await consultasMedicas.WaitAsync();
            try
            {
                // Simula la consulta
                paciente.Estado = EstadoPaciente.Consulta;
                Console.WriteLine($"Paciente {paciente.Id} inicia consulta. (Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad})");
                await Task.Delay(paciente.TiempoConsulta * 1000);
            }
            finally
            {
                consultasMedicas.Release();
            }

            // Si requiere diagnóstico, pasar a máquina de diagnóstico
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

            paciente.Estado = EstadoPaciente.Finalizado;
            int duracionTotal = paciente.TiempoConsulta + (paciente.RequiereDiagnostico? 15 : 0);
            Console.WriteLine($"Paciente {paciente.Id} finalizado. Duración total: {duracionTotal} segundos.");
        }

        static async Task GeneradorPacientes(int numeroPacientes)
        {
            // Genera pacientes con características aleatorias y los agrega a la cola
            int contador = 0;
            Random rnd = new Random();
            while (contador < numeroPacientes)
            {
                contador++;
                int ordenLlegada = contador;
                int tiempoConsulta = rnd.Next(5, 16);
                int id = rnd.Next(1, 101);
                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);
                bool requiereDiagnostico = rnd.Next(0, 2) == 1;
                
                Paciente paciente = new Paciente(id, contador * 2, tiempoConsulta, prioridad, requiereDiagnostico, ordenLlegada);
                
                _ = Task.Run(() => AtenderPaciente(paciente, 0));

                Console.WriteLine($"[Generador] Paciente {paciente.Id} generado. Orden: {paciente.OrdenLlegada}, Prioridad: {paciente.NivelPrioridad}");
                await Task.Delay(2000);
            }
        }

        static async Task Main(string[] args)
        {
            // Configuración inicial
            int numeroPacientes = 50;
            DateTime inicioSimulacion = DateTime.Now;

            await GeneradorPacientes(numeroPacientes);
            await Task.Delay(30000); // Esperar tiempo suficiente para que terminen todos los pacientes

            DateTime finSimulacion = DateTime.Now;
            double duracionSimulacion = (finSimulacion - inicioSimulacion).TotalSeconds;
            double usoDiagnostico = (tiempoTotalDiagnostico / (2 * duracionSimulacion)) * 100;

            double promEmergencia = tiemposEspera[1].Any() ? tiemposEspera[1].Average() : 0;
            double promUrgencia = tiemposEspera[2].Any() ? tiemposEspera[2].Average() : 0;
            double promConsulta   = tiemposEspera[3].Any() ? tiemposEspera[3].Average() : 0;

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
