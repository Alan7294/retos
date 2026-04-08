using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pw2_clase5.Models
{
    [Table("soluciones")]
    public class Solucion
    {
        [Key]
        [Column("id_solucion")]
        public int IdSolucion { get; set; }

        [Required]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [Column("fecha_envio")]
        public DateTime FechaEnvio { get; set; } = DateTime.Now;

        [Column("id_usuario")]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [Column("id_reto")]
        public int IdReto { get; set; }
        [ForeignKey("IdReto")]
        public virtual Reto Reto { get; set; }

        public bool Activo { get; set; } = true;
    }
}
