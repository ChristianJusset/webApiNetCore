namespace WebApiPelicula.DTOs.SalaCine
{
    public class SalaDeCineCercanoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double DistanciaEnMetros { get; set; }
    }
}
