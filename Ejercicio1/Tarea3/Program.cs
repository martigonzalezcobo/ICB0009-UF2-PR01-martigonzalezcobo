using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    public enum EstadoPaciente
    {
        Espera,
        Consulta,
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
        public int Id { get; set; }
        public int LlegadaHospital { get; set; }
        public int TiempoConsulta { get; set; }
        public EstadoPaciente Estado { get; set; }
        public PrioridadPaciente Prioridad { get; set; }

        public Paciente(int id, int llegadaHospital, int tiempoConsulta, PrioridadPaciente prioridad)
        {
            this.Id = id;
            this.LlegadaHospital = llegadaHospital;
            this.TiempoConsulta = tiempoConsulta;
            this.Estado = EstadoPaciente.Espera;
            this.Prioridad = prioridad;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            int numeroPacientes = 4;
            Random rnd = new Random();

            for (int i = 0; i < numeroPacientes; i++)
            {
                int ordenLlegada = i + 1;
                int llegada = i * 2;
                int tiempoConsulta = rnd.Next(5, 16);
                int id = rnd.Next(1, 101);

                PrioridadPaciente prioridad = (PrioridadPaciente)rnd.Next(0, 3);

                Paciente paciente = new Paciente(id, llegada, tiempoConsulta, prioridad);

                Task tarea = Task.Run(() =>
                {
                    Thread.Sleep(llegada * 1000);

                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Espera. Prioridad: {paciente.Prioridad}. Duración Espera: 0 segundos.");

                    int duracionEspera = 0;
                    paciente.Estado = EstadoPaciente.Consulta;
                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Consulta. Prioridad: {paciente.Prioridad}. Duración Espera: {duracionEspera} segundos.");

                    Thread.Sleep(paciente.TiempoConsulta * 1000);

                    paciente.Estado = EstadoPaciente.Finalizado;
                    Console.WriteLine($"Paciente {paciente.Id}. Llegado el {ordenLlegada}. Estado: Finalizado. Prioridad: {paciente.Prioridad}. Duración Consulta: {paciente.TiempoConsulta} segundos.");
                });

                tareas.Add(tarea);
            }

            Task.WaitAll(tareas.ToArray());
            Console.WriteLine("Todas las consultas han finalizado.");
        }
    }
}
