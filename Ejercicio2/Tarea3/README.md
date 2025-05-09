# Ejercicio #2 – Unidades de Diagnóstico – Tarea #3

## Descripción

En esta tarea, se amplía el número de pacientes a **20** mientras se mantienen las **4 consultas médicas con 4 médicos** y las **2 máquinas de diagnóstico**. Los pacientes llegarán de forma secuencial, **uno cada 2 segundos**. 

Si todas las consultas médicas están ocupadas, los pacientes deberán esperar en estado **"EsperaConsulta"** hasta que haya una consulta disponible.

### Reglas del sistema:
- **4 consultas médicas** disponibles.
- **2 máquinas de diagnóstico**.
- **20 pacientes** llegarán secuencialmente, cada **2 segundos**.
- Si no hay consultas libres, los pacientes esperan en **"EsperaConsulta"**.
- El paciente es atendido y, tras su consulta, se libera su puesto para el siguiente en la cola.

---

## Preguntas y Respuestas

### Explica el planteamiento de tu código y plantea otra posibilidad de solución a la que has programado y por qué has escogido la tuya.

**Respuesta:**

> La nueva variable **`consultasMedicas`** permite que hasta **4 pacientes sean atendidos simultáneamente**. 
>
> - Si llega un nuevo paciente y **no hay consultas libres**, su estado se mantiene como **"EsperaConsulta"**.
> - Cuando un paciente es atendido, su consulta dura **(tiempoConsulta * 1000) milisegundos**.
> - Luego, el paciente es sacado de la consulta con **`consultasMedicas.Release()`** para permitir el ingreso del siguiente.
>
> **Otra solución posible:**
>
> - Usar una **estructura de datos concurrente** para manejar una **cola de pacientes**.
> - Si no hay consultas disponibles, los pacientes se almacenan en una **cola ordenada por llegada**.
> - Un hilo gestiona la cola, asignando pacientes a consultas a medida que quedan libres.
> - Otro hilo maneja la prioridad, tiempos de espera y estados de consulta.

### ¿Los pacientes que deben esperar entran luego a la consulta por orden de llegada? Explica qué tipo de pruebas has realizado para comprobar este comportamiento.

**Respuesta:**

> Sí, los pacientes **entran en la consulta por orden de llegada**.
>
> - Para comprobarlo, realicé **dos simulaciones**, observando que los pacientes se asignan a las consultas en el orden esperado.

---

![ejercicio2tarea3-1](https://github.com/user-attachments/assets/4062c9ff-f35a-4fc9-9f0a-f3963a434ad7)
![ejercicio2tarea3-2](https://github.com/user-attachments/assets/3ccf2fae-d181-4a1e-a65c-ef52b7159ff2)
![ejercicio2tarea3-3](https://github.com/user-attachments/assets/c698a07e-5847-416a-85da-95d56e786f76)
![ejercicio2tarea3-4](https://github.com/user-attachments/assets/b76d1417-c7ad-46d6-9ee7-356cfac463bd)
