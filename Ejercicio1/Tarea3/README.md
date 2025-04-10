# Ejercicio #1 – Visualización del Avance – Tarea #3

## Descripción

El objetivo de esta tarea es mostrar por consola la información de cada paciente, junto con sus cambios de estado, el orden de llegada y el tiempo transcurrido entre cambios de estado. 

Cada vez que un paciente cambie de estado, se debe visualizar la siguiente información en la consola:

```plaintext
Paciente <Id>. Llegado el N. Estado: <Estado>. Duración: <S> segundos.
```

### Ejemplo de visualización:
```plaintext
Paciente 12. Llegado el 1. Estado: Finalizado. Duración Consulta: 10 segundos.
Paciente 34. Llegado el 2. Estado: Consulta. Duración Espera: 0 segundos.
Paciente 53. Llegado el 3. Estado: Consulta. Duración Espera: 0 segundos.
Paciente 21. Llegado el 4. Estado: Consulta. Duración Espera: 0 segundos.
Paciente 12. Llegado el 1. Estado: Consulta. Duración Espera: 0 segundos.
```

---

## Información Adicional

### ¿Has decidido visualizar información adicional a la planteada en el ejercicio? ¿Por qué?

**Respuesta:**

> Sí, he decidido añadir un nuevo parámetro que simboliza la **prioridad** del paciente según la gravedad de la consulta. Puede ser una **emergencia, una urgencia o una consulta general**.

> - Si es una **consulta general**, podría añadirse qué **médico** está atendiendo al paciente.
> - Si es una **emergencia**, se podría mostrar cuánto tiempo ha pasado desde la llegada del paciente hasta que fue atendido.
> - Para una **urgencia**, también se podría mostrar este tiempo de espera.
> - Si el paciente **llega en ambulancia**, el tiempo de espera podría omitirse, ya que se supone que ha sido atendido desde el inicio.

---

![ejercicio1tarea3](https://github.com/user-attachments/assets/f7eeb45e-31d7-4961-bd13-ae96a27fdda4)
