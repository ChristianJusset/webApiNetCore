namespace NetCoreApi.DTOs.Seguridad
{
    public class ResultadoHash
    {
        public string Hash { get; set; }
        public byte[] Sal { get; set; }
    }
}
