using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pw2_clase5.Models
{
	[Table("retos")]
	public class Reto
	{
		[Key]
		[Column("id_reto")]
		public int IdReto { get; set; }

		[Required]
		[Column("titulo")]
		public string Titulo { get; set; } = string.Empty;

		[Required]
		[Column("descripcion")]
		public string Descripcion { get; set; } = string.Empty;

		[Required]
		[Column("dificultad")]
		public string Dificultad { get; set; } = string.Empty;

		[Required]
		[Column("puntos")]
		public int Puntos { get; set; }

		[Column("fecha_creacion")]
		public DateTime FechaCreacion { get; set; } = DateTime.Now;

		public virtual ICollection<Solucion> Soluciones { get; set; } = new List<Solucion>();

        public bool Activo { get; set; } = true;

    }
}
