# Ejercicio #2 – Unidades de Diagnóstico – Tarea #2

## Descripción

En esta tarea, se implementará una sincronización para realizar las pruebas de diagnóstico. Ahora, los pacientes deberán pasar por orden de llegada al hospital, sin importar el orden en que salieron de la consulta.

### Reglas del sistema:
- El **paciente 1** (el primero en llegar) será el primero en realizar la prueba, seguido del **paciente 2**, luego el **paciente 3** y finalmente el **paciente 4**.
- **El orden de entrada a la prueba de diagnóstico debe mantenerse**, independientemente del tiempo que cada paciente haya pasado en consulta.
- Se mantiene el uso de **2 máquinas** de diagnóstico, con un máximo de **un paciente por máquina**.

### Ejemplo de comportamiento:
Si el **paciente 1** es el que más tiempo ha pasado en consulta, **todos los demás deberán esperar** a que él realice las pruebas antes que ellos.

---

## Preguntas y Respuestas

### Explica la solución planteada en tu código y por qué la has escogido.

**Respuesta:**

> Mi solución está basada en dos partes:
>
> - **Control de acceso con `SemaphoreSlim`**: Se utiliza para limitar el número de pacientes que pueden usar las máquinas simultáneamente a **dos**.
> - **Uso de variables `pacienteEnTurno` y `bloqueoOrden`**: 
>   - `pacienteEnTurno` asigna un número a cada paciente para determinar su orden de paso.
>   - Con `Monitor.Wait` y `Monitor.PulseAll`, cada paciente espera su turno antes de iniciar la prueba.

### Plantea otra posibilidad de solución a la que has programado.

**Respuesta:**

> Otra alternativa sería usar una **cola**. 
>
> - Cada paciente que entre a consulta se registraría en una **estructura de datos ordenada por turno de llegada**.
> - Se usarían **dos hilos**:
>   1. **Hilo encargado de manejar la cola**: Asigna las máquinas de diagnóstico en orden.
>   2. **Hilo encargado de procesar estados**: Gestiona el tiempo de espera, la gravedad de la urgencia y el estado de la consulta.

---

![Captura de pantalla 2025-03-29 210007](https://github.com/user-attachments/assets/3052134c-fe27-48c1-91a5-aadc22d63da6)
