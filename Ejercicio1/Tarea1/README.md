# Ejercicio #1 – Consulta médica – Tarea #1

## Descripción

En esta tarea hay que simular la llegada de 4 pacientes al hospital y su atención médica en las 4 consultas médicas disponibles.

- Llega un paciente cada **2 segundos**.
- Hay **4 médicos**.
- Un médico atiende a los pacientes de forma **individual**.
- El tiempo de atención es siempre de **10 segundos**.
- Los médicos se asignarán de forma **aleatoria** del 1 al 4.
- Si un médico ya está atendiendo a un paciente, **no podrá atender a otro simultáneamente**.

Cada vez que llega un paciente se mostrará un mensaje por pantalla junto al número de llegada (**Paciente 1, 2, 3 o 4**). Cada vez que un paciente sale de consulta, también se mostrará un mensaje por pantalla.

---

## Preguntas y Respuestas

### 1. ¿Cuántos hilos se están ejecutando en este programa? Explica tu respuesta.

> Hay cinco hilos en total. El principal se encarga de gestionar la llegada de pacientes y los otros cuatro hilos restantes se encargan cada uno de atender a cada paciente.

### 2. ¿Cuál de los pacientes entra primero en consulta? Explica tu respuesta.

> Como los médicos se asignan aleatoriamente, no hay un orden fijo. El primer paciente que llegue y encuentre un médico libre será el primero en ser atendido.

### 3. ¿Cuál de los pacientes sale primero de consulta? Explica tu respuesta.

> El primer paciente que entre en consulta será el primero en salir ya que todos tienen el mismo tiempo de consulta (10 segundos).

---
![ejercicio1tarea1](https://github.com/user-attachments/assets/23b5bbd3-c274-462d-a10e-5e00572d02ce)
