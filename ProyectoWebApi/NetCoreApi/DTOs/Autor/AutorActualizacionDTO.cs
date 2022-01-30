using NetCoreApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace NetCoreApi.DTOs.Autor
{
    public class AutorActualizacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe de tener más de {1} carácteres")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}
