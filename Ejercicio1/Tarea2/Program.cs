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

        public Paciente(int id, int llegadaHospital, int tiempoConsulta)
        {
            this.Id = id;
            this.LlegadaHospital = llegadaHospital;
            this.TiempoConsulta = tiempoConsulta;
            this.Estado = "Espera";
        }

        public override string ToString()
        {
            return $"Paciente {Id} (Llegada: {LlegadaHospital}s, Consulta: {TiempoConsulta}s)";
        }
    }

    class Program
    {   
        static Random rnd = new Random();

        static void AtenderPaciente(Paciente paciente, int numeroLlegada, int prioridad)
        {
            paciente.Estado = "Consulta";
            Console.WriteLine($"{paciente} - Número de llegada: {numeroLlegada} - Prioridad: {prioridad} -> Entra en consulta.");
         
            Thread.Sleep(paciente.TiempoConsulta * 1000);
            
            paciente.Estado = "Finalizado";
            Console.WriteLine($"{paciente} - Número de llegada: {numeroLlegada} - Prioridad: {prioridad} --> Finaliza consulta.");
        }

        static void Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4;

            for (int i = 0; i < numeroPacientes; i++)
            {
                int llegada = i * 2;

                int tiempoConsulta = rnd.Next(5, 16);

                int id = rnd.Next(1, 101);

                Paciente paciente = new Paciente(id, llegada, tiempoConsulta);
                int numeroLlegada = i + 1;

                int prioridad = rnd.Next(1, 4);

                Task tarea = Task.Run(() =>
                {
                    Thread.Sleep(llegada * 1000);
                    AtenderPaciente(paciente, numeroLlegada, prioridad);
                });

                tareas.Add(tarea);
            }

            Task.WaitAll(tareas.ToArray());
            Console.WriteLine("Todas las consultas han finalizado.");
        }
    }
}
