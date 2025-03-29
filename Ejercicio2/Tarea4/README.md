# Ejercicio #2 – Prioridades de los Pacientes – Tarea #4

## Descripción

En esta tarea, se introduce un **nivel de prioridad** para los pacientes al llegar al hospital. Existen **tres niveles de prioridad**:

1. **Emergencias (nivel 1)** → Atendidos primero.
2. **Urgencias (nivel 2)** → Atendidos después de las emergencias.
3. **Consultas generales (nivel 3)** → Atendidos al final.

### Reglas del sistema:
- Se amplía la clase **Paciente** con el atributo **prioridad (int)** para indicar la prioridad del paciente.
- Los pacientes en espera **entran por orden de prioridad**.
- Si hay varios pacientes con el mismo nivel de prioridad, se atienden en **orden de llegada**.

---

## Preguntas y Respuestas

### Explica el planteamiento de tu código y plantea otra posibilidad de solución a la que has programado y por qué has escogido la tuya.

**Respuesta:**

> He añadido un **nivel de prioridad** que se asigna a cada paciente según la gravedad de su caso.
>
> - Además, con el valor de **orden de llegada**, me aseguro de que si dos pacientes tienen el mismo nivel de prioridad, se atiendan en el orden correcto.
> - Se ha creado una **cola de pacientes** y un **lock** para garantizar que solo un hilo pueda modificarla simultáneamente.
> - El método **`EsPrimerPaciente`** organiza a los pacientes determinando quién es el primero en la cola según:
>   - **Mayor prioridad** (nivel 1 antes que 2 y 3).
>   - **Orden de llegada** (en caso de empate de prioridad).
>
> **Otra solución posible:**
>
> - Utilizar una **estructura de datos de prioridad** (como una **PriorityQueue** o un **SortedList**) para gestionar el orden de atención.
> - Un hilo monitorea la cola y asigna pacientes a consultas según el criterio de prioridad.
> - Esto permitiría una gestión más eficiente de los pacientes en espera.

---

![Captura de pantalla 2025-03-29 212300](https://github.com/user-attachments/assets/c8875ebc-4d92-49a0-a0eb-32e08fb7d523)
