-- Conceder privilegios en keycloak
GRANT ALL PRIVILEGES ON DATABASE keycloak TO postgres;

-- Conectar a keycloak y conceder privilegios en schema public
\c keycloak
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO postgres;

SELECT 'Privilegios asignados correctamente' as mensaje;