# Ejercicio #2 – Unidades de Diagnóstico – Tarea #1

## Descripción

En esta tarea, se simula un hospital donde algunos pacientes requieren realizar pruebas en equipos limitados, como un escáner corporal. El hospital dispone de **2 máquinas** de diagnóstico, y solo **un paciente** puede usarlas a la vez.

### Reglas del sistema:
- Se amplía la clase `Paciente` con el atributo `requiereDiagnostico` (booleano), que se genera **aleatoriamente**.
- Los pacientes **entran primero en consulta** y luego, si es necesario, **realizan pruebas de diagnóstico**.
- La simulación de las pruebas de diagnóstico tiene un tiempo adicional de **15 segundos**.
- Se amplía la clase `Estado` con los siguientes estados:
  - **EsperaConsulta:** Ha llegado al hospital, pero aún no ha entrado en consulta.
  - **Consulta:** Ha entrado en consulta, pero aún no ha salido.
  - **EsperaDiagnostico:** Ha finalizado la consulta y requiere diagnóstico, pero aún no ha sido asignado a una máquina.
  - **Finalizado:** Ha finalizado la consulta y el diagnóstico (si era necesario).

---

## Visualización del Avance

Se debe actualizar la visualización para reflejar los cambios de estado de los pacientes, por ejemplo:

```plaintext
Paciente 8. Llegado el 1. Estado: EsperaConsulta.
Paciente 12. Llegado el 2. Estado: Consulta.
Paciente 34. Llegado el 3. Estado: EsperaDiagnostico.
Paciente 21. Llegado el 4. Estado: Finalizado.
```

---

## Pregunta y Respuesta

### ¿Los pacientes que deben esperar para hacerse las pruebas de diagnóstico entran luego a hacerse las pruebas por orden de llegada? Explica qué tipo de pruebas has realizado para comprobar este comportamiento.

**Respuesta:**

> Sí, pero el sistema **First in, First out (FIFO)** no garantiza que el primer paciente que entre sea el primero que salga. Esto depende del sistema de planificación de tareas.
>
> He realizado **4 pruebas** ejecutando el código y, en todas ellas, el paciente que ha necesitado la máquina ha sido el primero en entrar y salir.

---

![ejercicio2tarea1-1](https://github.com/user-attachments/assets/530f809f-f527-4d11-af24-86fedeb28bfb)
![ejercicio2tarea1-2](https://github.com/user-attachments/assets/817c8c58-f535-4e68-97a2-4256dc2e6549)
![ejercicio2tarea1-3](https://github.com/user-attachments/assets/aeb7c304-8a1c-46ec-9d65-e0798df6a831)
