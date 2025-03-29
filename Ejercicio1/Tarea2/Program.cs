using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    public class Paciente
    {
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public string Estado { get; set; }

        // Constructor que inicializa las propiedades básicas del paciente.
        public Paciente(int id, int llegadaHospital, int tiempoConsulta)
        {
            this.Id = id;
            this.LlegadaHospital = llegadaHospital;
            this.TiempoConsulta = tiempoConsulta;
            this.Estado = "Espera"; // Estado inicial asignado al paciente.
        }

        // Método para mostrar la información del paciente.
        public override string ToString()
        {
            return $"Paciente {Id} (Llegada: {LlegadaHospital}s, Consulta: {TiempoConsulta}s)";
        }
    }

    class Program
    {   
        // Objeto Random para generar datos aleatorios (ID, duración de consulta, prioridad).
        static Random rnd = new Random();

        // Método que simula la atención de un paciente.
        // Se actualiza el estado del paciente y se simula el tiempo de consulta.
        static void AtenderPaciente(Paciente paciente, int numeroLlegada, int prioridad)
        {
            paciente.Estado = "Consulta"; // Cambia el estado a "Consulta".
            Console.WriteLine($"{paciente} - Número de llegada: {numeroLlegada} - Prioridad: {prioridad} -> Entra en consulta.");
         
            // Simula la duración de la consulta (multiplicamos por 1000 para convertir a milisegundos).
            Thread.Sleep(paciente.TiempoConsulta * 1000);
            
            paciente.Estado = "Finalizado"; // Cambia el estado a "Finalizado" tras la consulta.
            Console.WriteLine($"{paciente} - Número de llegada: {numeroLlegada} - Prioridad: {prioridad} --> Finaliza consulta.");
        }

        static void Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4; // Número total de pacientes a simular.

            // Se crean tareas para cada paciente, simulando una llegada escalonada.
            for (int i = 0; i < numeroPacientes; i++)
            {
                int llegada = i * 2; // Cada paciente llega con un retraso de 2 segundos.
                int tiempoConsulta = rnd.Next(5, 16); // Duración aleatoria de la consulta entre 5 y 15 segundos.
                int id = rnd.Next(1, 101); // ID aleatorio entre 1 y 100.

                Paciente paciente = new Paciente(id, llegada, tiempoConsulta);
                int numeroLlegada = i + 1; // Número secuencial de llegada.
                int prioridad = rnd.Next(1, 4); // Prioridad aleatoria entre 1 y 3.

                // Se lanza la atención del paciente de forma asíncrona.
                Task tarea = Task.Run(() =>
                {
                    // Simula el retraso de llegada del paciente.
                    Thread.Sleep(llegada * 1000);
                    AtenderPaciente(paciente, numeroLlegada, prioridad);
                });

                tareas.Add(tarea); // Se añade la tarea a la lista.
            }

            // Espera a que todas las tareas finalicen antes de terminar el programa.
            Task.WaitAll(tareas.ToArray());
            Console.WriteLine("Todas las consultas han finalizado.");
        }
    }
}
