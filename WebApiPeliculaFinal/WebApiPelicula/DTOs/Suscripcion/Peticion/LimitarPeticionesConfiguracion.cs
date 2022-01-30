namespace WebApiPelicula.DTOs.Suscripcion.Peticion
{
    public class LimitarPeticionesConfiguracion
    {
        public int PeticionesPorDiaGratuito { get; set; }
        public string[] ListaBlancaRutas { get; set; }
    }
}
