# Ejercicio #2 – Estadísticas y Logs – Tarea #5

## Descripción

En esta tarea, al final de la simulación, se mostrarán estadísticas clave sobre la atención médica:

### Estadísticas Generadas:
- **Número total de pacientes atendidos por prioridad.**
- **Tiempo promedio de espera por paciente.**
- **Uso promedio de las máquinas de diagnóstico.**

#### Ejemplo de salida:
```
--- FIN DEL DÍA ---
Pacientes atendidos:
- Emergencias: 5
- Urgencias: 10
- Consultas generales: 8

Tiempo promedio de espera:
- Emergencias: 2s
- Urgencias: 4s
- Consultas generales: 6s

Uso promedio de máquinas de diagnóstico: 75%
```

---

## Preguntas y Respuestas

### ¿Puedes explicar tu código y por qué has decidido hacerlo así?

**Respuesta:**

> - He añadido **tres variables globales** para mantener un recuento del total de emergencias, urgencias y consultas generales. Estas se incrementan según el atributo `NivelPrioridad`.
> - Se ha agregado una propiedad para registrar la **hora de llegada** del paciente y el **inicio de su consulta**, lo que permite calcular con precisión el tiempo de espera.
> - También se recoge el **tiempo total que ha durado el diagnóstico** para calcular el uso promedio de las máquinas de diagnóstico.
> - La elección de este enfoque permite una gestión eficiente de los datos sin afectar el rendimiento del sistema.

---

![Captura de pantalla 2025-03-29 212359](https://github.com/user-attachments/assets/3336a93c-086e-4229-943d-a149875438f4)
