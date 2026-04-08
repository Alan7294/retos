Proyecto: RetosCode - Plataforma de Retos de Programación

## UNIVERSIDAD PRIVADA DOMINGO SAVIO
### FACULTAD DE INGENIERÍA | CARRERA DE INGENIERÍA DE SISTEMAS
### PROGRAMACIÓN WEB II - SEXTO SEMESTRE

---

**ESTUDIANTES:**
* Alan Joaquin Laura Paz

**DOCENTE:** * Albino Chambi Andrés Grover

**FECHA:** 08/04/2026 | **UBICACIÓN:** La Paz - Bolivia

---

## 1. INTRODUCCIÓN

### 1.1. Contexto General
En el ámbito de la educación tecnológica y la evaluación de habilidades de programación, es fundamental contar con herramientas que permitan a los estudiantes practicar y a los docentes evaluar competencias técnicas de manera estructurada. Actualmente, muchas instituciones utilizan plataformas externas o métodos manuales para gestionar ejercicios de programación, lo que dificulta el seguimiento del progreso y la evaluación automatizada.

### 1.2. Problemática
El problema central es la **falta de una plataforma interna** para gestionar retos de programación, evaluar soluciones y mantener un registro competitivo del avance de los estudiantes.

**Subproblemas identificados:**
1. **Gestión de Retos:** No existe un sistema centralizado para crear, modificar y eliminar retos de programación con diferentes niveles de dificultad.
2. **Evaluación de Soluciones:** La evaluación de código se realiza de forma manual, consume tiempo y carece de estandarización.
3. **Seguimiento de Progreso:** Ausencia de un sistema de puntajes y Rankings que motive la participación activa de los estudiantes.
4. **Control de Acceso:** Necesidad de distinguir entre usuarios regulares y administradores para la gestión de contenidos.

### 1.3. Justificación
* **Técnica:** Se aplican tecnologías modernas como **ASP.NET Core 9** y **Angular 21** con arquitectura moderna (signals, standalone components), integrando **JWT** para la seguridad de la API y **Docker** para la portabilidad.
* **Operativa:** Automatiza la gestión de retos y evaluaciones, reduciendo la carga administrativa de los docentes.
* **Económica:** Al ser una solución propia, se evitan costos de licencias de plataformas externas como HackerRank o LeetCode.

---

## 2. OBJETIVOS

### 2.1. Objetivo General
Desarrollar una Plataforma de Retos de Programación (RetosCode) que permita crear, gestionar y resolver retos de programación de manera eficiente, con evaluación automatizada y un sistema de Rankings, utilizando ASP.NET Core 9, Angular 21 y PostgreSQL.

### 2.2. Objetivos Específicos
* Implementar el backend con ASP.NET Core siguiendo buenas prácticas de desarrollo.
* Implementar el frontend con Angular 21 para la gestión visual de retos y soluciones.
* Diseñar un sistema de autenticación y autorización basado en JWT.
* Implementar un sistema de puntajes y Rankings por usuario.
* Configurar Docker para el despliegue integral de la aplicación.

---

## 4. ANÁLISIS DEL SISTEMA

### 4.1. Requerimientos Funcionales (Historias de Usuario)

#### Módulo de Autenticación
* **COMO:** Estudiante **QUIERO:** registrarme en la plataforma **PARA:** acceder a los retos disponibles.
* **COMO:** Estudiante **QUIERO:** iniciar sesión **PARA:** guardar mi progreso y puntaje.
* **COMO:** Administrador **QUIERO:** acceder al panel de administración **PARA:** gestionar retos y evaluar soluciones.

#### Módulo de Retos
* **COMO:** Administrador **QUIERO:** crear un nuevo reto con título, descripción, dificultad y puntaje **PARA:** ampliar la biblioteca de ejercicios.
* **COMO:** Estudiante **QUIERO:** ver la lista de retos disponibles **PARA:** seleccionar cuál resolver.
* **COMO:** Estudiante **QUIERO:** enviar mi solución a un reto **PARA:** obtener retroalimentación.

#### Módulo de Evaluaciones
* **COMO:** Administrador **QUIERO:** revisar las soluciones enviadas por los estudiantes **PARA:** aprobarlas o rechazarlas.
* **COMO:** Administrador **QUIERO:** aprobar una solución correcta **PARA:** otorgar los puntos correspondientes al estudiante.

#### Módulo de Rankings
* **COMO:** Estudiante **QUIERO:** ver el Ranking de usuarios por puntaje **PARA:** competir y motivarme a mejorar.

---

## 5. DISEÑO DEL SISTEMA

### 5.1. Modelo Relacional

#### Tabla: Usuarios

| Campo | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| IdUsuario | INT | PK, AUTO_INCREMENT | Identificador único del usuario |
| Nombre | VARCHAR(100) | NOT NULL | Nombre del usuario |
| Email | VARCHAR(255) | UNIQUE, NOT NULL | Correo electrónico |
| Password | VARCHAR(255) | NOT NULL | Contraseña hasheada |
| PuntajeTotal | INT | DEFAULT 0 | Puntos acumulados |
| Rol | VARCHAR(20) | DEFAULT 'USER' | USER o ADMIN |

#### Tabla: Retos

| Campo | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| IdReto | INT | PK, AUTO_INCREMENT | Identificador único del reto |
| IdUsuario | INT | FK -> Usuarios(IdUsuario) | Creador del reto |
| Titulo | VARCHAR(200) | NOT NULL | Título del reto |
| Descripcion | TEXT | NOT NULL | Descripción del problema |
| Dificultad | VARCHAR(20) | NOT NULL | Facil, Medio, Dificil |
| Puntos | INT | NOT NULL | Puntaje del reto |

#### Tabla: Soluciones

| Campo | Tipo | Restricciones | Descripción |
| :--- | :--- | :--- | :--- |
| IdSolucion | INT | PK, AUTO_INCREMENT | Identificador de la solución |
| IdReto | INT | FK -> Retos(IdReto) | Reto que se está resolviendo |
| IdUsuario | INT | FK -> Usuarios(IdUsuario) | Estudiante que envía la solución |
| Codigo | TEXT | NOT NULL | Código fuente enviado |
| Descripcion | TEXT | nullable | Descripción opcional |
| Estado | VARCHAR(20) | DEFAULT 'pendiente' | pendiente, correcto, incorrecto |
| FechaEnvio | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | Momento del envío |

---

## 6. IMPLEMENTACIÓN

### 6.1. Tech Stack
* **Frontend:** Angular 21 con señales (signals), standalone components, TypeScript
* **Backend:** ASP.NET Core 9 con C#
* **Base de Datos:** PostgreSQL
* **Autenticación:** JWT (JSON Web Tokens)
* **Contenedores:** Docker y Docker Compose

### 6.2. Estructura del Proyecto

```
├── Backend/
│   ├── Controllers/      # Endpoints de la API
│   ├── Models/          # Entidades de la base de datos
│   ├── Services/        # Lógica de negocio
│   ├── Data/            # Contexto de Entity Framework
│   └── Program.cs      # Configuración de la app
│
├── frontend/
│   ├── src/app/
│   │   ├── components/  # Componentes reutilizables
│   │   ├── pages/       # Páginas de la aplicación
│   │   ├── services/    # Servicios HTTP
│   │   ├── guards/      # Protectores de rutas
│   │   └── interceptors/# Interceptores HTTP
│   └── Dockerfile
│
└── docker-compose.yml   # Orquestación de contenedores
```

### 6.3. Configuración de Entorno (Docker Compose)
El sistema se orquesta mediante contenedores para garantizar la paridad entre desarrollo y producción:
* **Base de Datos:** Contenedor `postgres:16-alpine` persistiendo datos en volúmenes.
* **Backend:** Imagen generada a partir del Dockerfile en la carpeta `/Backend` exponiendo el puerto 5000.
* **Frontend:** Imagen generada a partir del Dockerfile en `/frontend` expuesta en el puerto 80.

---

## 7. CARACTERÍSTICAS IMPLEMENTADAS

1. ✅ Sistema de autenticación (Registro/Login)
2. ✅ CRUD completo de retos (crear, leer, actualizar, eliminar)
3. ✅ Envío de soluciones por parte de los estudiantes
4. ✅ Sistema de evaluación manual por administrador
5. ✅ Actualización automática de puntajes
6. ✅ Página de Rankings con ordenamiento por puntaje
7. ✅ Perfil de usuario con información y puntaje
8. ✅ Panel de administración para gestión de retos y soluciones
9. ✅ Protección de rutas mediante Guards
10. ✅ Interceptor JWT para autenticación en requests

---

## 8. CONCLUSIONES

1. **Arquitectura Moderna:** La implementación con Angular 21 y ASP.NET Core 9 garantizó un código escalable y fácil de mantener.
2. **Seguridad Implementada:** El uso de JWT para la autenticación permitió proteger los endpoints de la API y gestionar roles de usuario.
3. **Experiencia de Usuario:** La plataforma ofrece una interfaz intuitiva para la gestión de retos, envío de soluciones y seguimiento del progreso académico.
4. **Portabilidad:** Docker facilita el despliegue de la aplicación en cualquier entorno sin dependencia del sistema operativo.

---

## 9. REPOSITORIO

El código fuente del proyecto está disponible en:
**https://github.com/Alan7294/retos**