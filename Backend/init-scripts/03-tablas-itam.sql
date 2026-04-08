-- 1. Tabla: Categorias
CREATE TABLE categorias (
    id_categoria SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT
);

-- 2. Tabla: Activos
CREATE TABLE activos (
    id_activo UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    id_categoria INT NOT NULL,
    codigo_activo VARCHAR(50) UNIQUE NOT NULL,
    numero_serie VARCHAR(100) UNIQUE,
    estado_actual VARCHAR(30) CHECK (estado_actual IN ('DISPONIBLE', 'ASIGNADO', 'MANTENIMIENTO', 'BAJA')),
    detalles_tecnicos JSONB,
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_activo_categoria FOREIGN KEY (id_categoria) REFERENCES categorias(id_categoria)
);

-- 3. Tabla: Asignaciones
CREATE TABLE asignaciones (
    id_asignacion UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    id_activo UUID NOT NULL,
    id_usuario_asignado UUID NOT NULL,
    fecha_entrega TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_devolucion TIMESTAMP,
    condicion_entrega TEXT,
    
    CONSTRAINT fk_asignacion_activo FOREIGN KEY (id_activo) REFERENCES activos(id_activo)
);

-- 4. Tabla: Tickets de Soporte
CREATE TABLE tickets_soporte (
    id_ticket UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    id_activo UUID NOT NULL,
    id_usuario_reporta UUID NOT NULL,
    descripcion_falla TEXT NOT NULL,
    estado_ticket VARCHAR(30) DEFAULT 'ABIERTO' CHECK (estado_ticket IN ('ABIERTO', 'EN_REVISION', 'SOLUCIONADO')),
    fecha_apertura TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_ticket_activo FOREIGN KEY (id_activo) REFERENCES activos(id_activo)
);