-- Crear base de datos para Keycloak si no existe
CREATE DATABASE keycloak;

-- Conectar a la base de datos keycloak
\c keycloak

-- Mensaje de confirmación
SELECT 'Base de datos keycloak creada exitosamente' as mensaje;