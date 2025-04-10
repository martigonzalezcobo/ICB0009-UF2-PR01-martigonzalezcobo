# Ejercicio #3 – Pacientes Infinitos – Tarea #1

## Descripción

En este ejercicio se modifica la generación de pacientes para que haya un **generador variable**, el cual crea nuevos pacientes de manera periódica.

### Parte 1: Generación dinámica de pacientes
- Cada **2 segundos**, se genera un nuevo paciente con datos aleatorios:
  - **Tiempo de consulta**.
  - **Prioridad (Emergencia, Urgencia, Consulta general).**
  - **Si requiere diagnóstico con máquina o no.**

### Parte 2: Evaluación con diferentes volúmenes de pacientes
- Se ejecutarán pruebas con **50, 100 y 1000 pacientes**.
- Se analizará si las soluciones implementadas en el Ejercicio #2 funcionan correctamente o presentan problemas.

---

## Análisis de las pruebas

### **Tarea 1: ¿Cumple requisitos?**
**Respuesta:**
Sí, porque introduce nuevos pacientes de forma periódica y mantiene la cola de espera en orden. Se respeta el orden de llegada y los pacientes son atendidos correctamente.

**Pruebas realizadas:**
[Detalles de las pruebas]

---

### **Tarea 2: ¿Qué comportamientos no previstos detectas?**
**Respuesta:**
Cuando se incrementa el número de pacientes, el **tiempo de espera y el tiempo de consulta aumentan gradualmente**.
- Esto ralentiza la atención de nuevos pacientes.
- Se genera **congestión en la cola de espera** y **las máquinas de diagnóstico se saturan**.

**Pruebas realizadas:**
[Detalles de las pruebas]

---

### **Tarea 3: ¿Cómo adaptarías tu solución?**
**Respuesta:**


---

![Captura de pantalla 2025-03-29 215141](https://github.com/user-attachments/assets/f70e889e-8615-458b-b85b-4747b4a0a700)
![Captura de pantalla 2025-03-29 215138](https://github.com/user-attachments/assets/d8f84f29-79d0-40f9-bae1-abe9664addc5)
