using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace pw2_clase5.Models
{
    [Table("usuarios")]
    [Index(nameof(Nombre), IsUnique = true)]   // índice único para nombre
    [Index(nameof(Correo), IsUnique = true)]   // índice único para correo
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("puntaje_total")]
        public int PuntajeTotal { get; set; } = 0;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public virtual ICollection<Solucion> Soluciones { get; set; } = new List<Solucion>();

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
