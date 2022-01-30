using AutoMapper;
using NetCoreApi.DTOs.Autor;
using NetCoreApi.DTOs.Comentario;
using NetCoreApi.DTOs.Libro;
using NetCoreApi.Entidades;

namespace NetCoreApi.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            // fuente-destino
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();
            CreateMap<Autor, AutorLibroDTO>().ForMember(autorDTO => autorDTO.Libros, opciones => opciones.MapFrom(MapAutorLibrosDTO));

            // guarda los libros con sus autors(ids)
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));

            CreateMap<LibroActualizacionDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibrosActualizacion));


            CreateMap<Libro, LibroDTO>().ReverseMap();
            CreateMap<Libro, LibroComentarioDTO>().ReverseMap();
            CreateMap<Libro, LibroAutorDTO>().
                ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroAutorDTO));

            CreateMap<LibroPatchDTO, Libro>().ReverseMap();

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private List<LibroDTO> MapAutorLibrosDTO(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();
            
            if (autor.AutoresLibros == null) { return resultado; }
            
            foreach (var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    Id = autorLibro.LibroId,
                    Titulo = autorLibro.Libro.Titulo
                });
            }

            return resultado;

        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            // los ids que se envian junto con los libros para guardalos se esta convirtiendo en entidades
            var resultado = new List<AutorLibro>();
            if (libroCreacionDTO.AutoresIds == null) { return resultado; }
            foreach (var autorId in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = autorId });
            }

            return resultado;
        }

        private List<AutorLibro> MapAutoresLibrosActualizacion(LibroActualizacionDTO libroCreacionDTO, Libro libro)
        {
            // los ids que se envian junto con los libros para guardalos se esta convirtiendo en entidades
            var resultado = new List<AutorLibro>();
            if (libroCreacionDTO.AutoresIds == null) { return resultado; }
            foreach (var autorId in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro() { AutorId = autorId });
            }

            return resultado;
        }

        private List<AutorDTO> MapLibroAutorDTO(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();
            if (libro.AutoresLibros == null) { return resultado; }
            foreach (var autorlibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorlibro.AutorId,
                    Nombre = autorlibro.Autor.Nombre
                });
            }
            return resultado;
        }

    }
}
