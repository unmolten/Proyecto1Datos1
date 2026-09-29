# Guía de Documentación del Proyecto (Doxygen)

Este proyecto ha sido documentado completamente en **español**, siguiendo el estándar de **Doxygen / Javadoc** sin etiquetas XML para maximizar la legibilidad del código fuente tanto para estudiantes como para herramientas automáticas.

---

## 1. Estructura de Comentarios Utilizada

En lugar de utilizar etiquetas XML de C# como `<summary>`, `<param>`, etc., se emplearon etiquetas estándar de Doxygen y bloques limpios:

- `@file`: Describe el propósito del archivo fuente.
- `@class` / `@enum`: Identifica la clase o enumeración documentada.
- `@brief`: Resumen conciso de una sola línea sobre lo que hace la clase o método.
- `@param`: Explica el propósito de cada parámetro recibido.
- `@return`: Describe el valor retornado por la función.
- Comentarios de bloque e internos explicativos paso a paso para algoritmos y enlaces de listas circulares.

---

## 2. Cómo generar la documentación con Doxygen

Si tienes **Doxygen** instalado en tu computadora:

1. Abre una terminal (PowerShell o CMD) en la raíz del proyecto:
   ```powershell
   cd "C:\Users\Gabriel\Coding\AyEdD\Proyecto1Datos1"
   ```
2. Ejecuta el comando:
   ```powershell
   doxygen Doxyfile
   ```
3. Esto generará la carpeta `docs/html/`.
4. Abre el archivo `docs/html/index.html` en cualquier navegador web (Chrome, Edge, Firefox) para navegar por la documentación interactiva con buscador, árbol de clases, herencias y métodos.

---

## 3. Módulos y Clases Principales Documentados

| Módulo / Archivo | Descripción |
| :--- | :--- |
| [`Node.cs`](Proyecto-1/Node.cs) | Nodo de la lista circular doblemente enlazada con punteros `next` y `previous`. |
| [`Linked_List.cs`](Proyecto-1/Linked_List.cs) | Lista circular doblemente enlazada con inserciones, eliminaciones, rotación y recorrido. |
| [`Casilla.cs`](Proyecto-1/Casilla.cs) | Jerarquía polimórfica: `Casilla` (abstracta), `Propiedad`, `CasillaEvento` y `CasillaEspecial`. |
| [`CartaEvento.cs`](Proyecto-1/CartaEvento.cs) | Modelo de cartas de Fortuna y Arca Comunal junto al enumerador de efectos `TipoEfectoCarta`. |
| [`MazoCartas.cs`](Proyecto-1/MazoCartas.cs) | Generador de barajas, barajado aleatorio (Fisher-Yates) y rotación circular de cartas. |
| [`Jugador.cs`](Proyecto-1/Jugador.cs) | Modelo de participante: balance, nodo de posición en el tablero, inventario y cárcel. |
| [`JuegoMonopoly.cs`](Proyecto-1/JuegoMonopoly.cs) | Coordinador central del juego (Singleton), lógica de turnos, compras, rentas y red. |
| [`Transacciones.cs`](Proyecto-1/Transacciones.cs) | Registro contable, persistencia en `Almacenamiento.txt` y generación de reportes. |
| [`ConexionPico.cs`](Proyecto-1/ConexionPico.cs) | Administrador de conexión serial con la Raspberry Pi Pico en segundo plano. |
| [`Dados.cs`](Proyecto-1/Dados.cs) | Controlador para captura y decodificación de dados físicos y virtuales. |
| [`ObtenerID.cs`](Proyecto-1/ObtenerID.cs) | Lector de tarjetas RFID para validación y autorización de pagos. |
| [`Creador.cs`](Proyecto-1/Creador.cs) | Asistente de registro interactivo de jugadores por consola y asignación de tags RFID. |
| [`Program.cs`](Proyecto-1/Program.cs) | Punto de entrada del servidor, menú interactivo y bucle de turnos. |
| [`Proyecto1Cliente/Program.cs`](Proyecto1Cliente/Program.cs) | Cliente de consola TCP asíncrono para jugadores remotos. |
| [`Godot UI/scripts/`](Godot%20UI/scripts/) | Scripts de interfaz gráfica (red, HUD de turnos, HUD de conexión, modal RFID y propiedades). |
