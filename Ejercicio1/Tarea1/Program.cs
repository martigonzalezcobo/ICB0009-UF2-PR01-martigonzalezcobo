using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GestionAtencionHospitalaria
{
    class Program
    {
        static SemaphoreSlim semaforoMedicos = new SemaphoreSlim(4); // 4 médicos disponibles
        static Random rnd = new Random();

        static async Task AtenderPaciente(int numeroPaciente)
        {
            Console.WriteLine($"Paciente {numeroPaciente} ha llegado al hospital.");
            await Task.Delay(numeroPaciente * 2000); // Simula el retraso de llegada (2s por paciente)

            await semaforoMedicos.WaitAsync(); // Espera a que haya un médico disponible
            int medicoAsignado = rnd.Next(1, 5); // Asigna un médico aleatorio del 1 al 4
            Console.WriteLine($"Paciente {numeroPaciente} es atendido por el médico {medicoAsignado}.");

            await Task.Delay(10000); // Simula consulta médica de 10s

            Console.WriteLine($"Paciente {numeroPaciente} ha salido de consulta.");
            semaforoMedicos.Release(); // Libera al médico
        }

        static async Task Main(string[] args)
        {
            List<Task> tareas = new List<Task>();
            for (int i = 1; i <= 4; i++)
            {
                tareas.Add(AtenderPaciente(i));
            }

            await Task.WhenAll(tareas);
            Console.WriteLine("Todas las consultas han finalizado.");
        }
    }
}
