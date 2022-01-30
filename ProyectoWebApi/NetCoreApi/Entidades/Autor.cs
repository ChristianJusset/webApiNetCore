using NetCoreApi.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCoreApi.Entidades
{
    public class Autor: IValidatableObject
    {
        /*
            validacion del model binding: se da gracias al ApiController
            NotMapped: para que no realize math con la BD
            [PrimeraLetraMayuscula]  : se puede utilizar en cualquier campo
            IValidatableObject : para realizar las validaciones por modelo
            validaciones: 
            [Range(18, 120)] 
            [CreditCard]  
            [Url]
            [NotMapped] 

         */

        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        
        // EF: va respetar esta configuracion donde la tabla va a tener un maximo de 120
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe de tener más de {1} carácteres")]
        
        //[PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }


        // regla por validacion por modelo
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Nombre: campo que se va a validar 
            if (!string.IsNullOrEmpty(Nombre))
            {
                var primeraLetra = Nombre[0].ToString();

                if (primeraLetra != primeraLetra.ToUpper())
                {
                    yield return new ValidationResult("La primera letra debe ser mayúscula",
                            new string[] { nameof(Nombre) });
                }
            }
        }
    }
}
