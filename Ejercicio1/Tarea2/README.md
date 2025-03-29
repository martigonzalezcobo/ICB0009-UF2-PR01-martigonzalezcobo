# Ejercicio #1 – Pacientes con datos – Tarea #2

## Descripción

El objetivo de esta tarea es asignar datos específicos a cada paciente para que tengan comportamientos diferentes. Cada paciente tiene los siguientes atributos:

- **Identificador único:** Un número entero aleatorio entre **1 y 100**.
- **Tiempo de llegada al hospital:** En segundos, comenzando en **0** cuando llega el primer paciente.
- **Tiempo en consulta:** Un número aleatorio entre **5 y 15 segundos**.
- **Estado del paciente:**
  - **Espera:** Ha llegado al hospital pero aún no ha entrado en consulta.
  - **Consulta:** Ha entrado en consulta.
  - **Finalizado:** Ha finalizado la consulta.

La clase que representa a un paciente se define de la siguiente manera:

```csharp
public class Paciente
{
    public int Id {get; set;}
    public int LlegadaHospital {get; set;}
    public int TiempoConsulta {get; set;}
    public int Estado {get; set;}

    public Paciente (int Id, int LlegadaHospital, int TiempoConsulta)
    {
        this.Id = Id;
        this.LlegadaHospital = LlegadaHospital;
        this.TiempoConsulta = TiempoConsulta;
    }
}
```

## Funcionamiento

- Ahora los pacientes estarán en consulta el tiempo indicado en **TiempoConsulta**.
- Se mostrará por pantalla:
  - **Id del paciente**
  - **Prioridad**
  - **Número de llegada** (orden en el que llegó: 1, 2, 3, 4...)

---

## Pregunta y Respuesta

### ¿Cuál de los pacientes sale primero de consulta? Explica tu respuesta.

**Respuesta:**

> Debido a la asignación aleatoria de tiempos de consulta y tiempos de llegada, el paciente que termine primero será aquel cuya **suma de llegada + tiempo de consulta** sea la menor.

---
![ejercicio1tarea2](https://github.com/user-attachments/assets/1e5e01b6-4906-4e7e-ac9d-1f89385dc075)
