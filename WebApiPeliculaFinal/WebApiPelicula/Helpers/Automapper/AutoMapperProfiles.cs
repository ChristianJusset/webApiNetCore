﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using WebApiPelicula.DTOs.Actor;
using WebApiPelicula.DTOs.Genero;
using WebApiPelicula.DTOs.Pelicula;
using WebApiPelicula.DTOs.Review;
using WebApiPelicula.DTOs.SalaCine;
using WebApiPelicula.DTOs.Suscripcion.Llave;
using WebApiPelicula.DTOs.Suscripcion.Restriccion;
using WebApiPelicula.DTOs.Usuario;
using WebApiPelicula.Entidades;
using WebApiPelicula.Entidades.Suscripciones;

namespace WebApiPelicula.Helpers.Automapper
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<GeneroCreacionDTO, Genero>();
            CreateMap<GenerActualizacionDTO, Genero>().ReverseMap();


            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());

            CreateMap<ActorActualizacionDTO, Actor>().ReverseMap();
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();


            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                             .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGenerosCreacion))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActoresCreacion));

            CreateMap<PeliculaActualizacionDTO, Pelicula>().ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGenerosActualizacion))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActoresActualizacion));

            CreateMap<Pelicula, PeliculaDetallesDTO>()
              .ForMember(x => x.Generos, options => options.MapFrom(MapPeliculasGeneros))
              .ForMember(x => x.Actores, options => options.MapFrom(MapPeliculasActores));

            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();


            CreateMap<SalaDeCine, SalaDeCineDTO>()
               .ForMember(x => x.Latitud, x => x.MapFrom(y => y.Ubicacion.Y))
               .ForMember(x => x.Longitud, x => x.MapFrom(y => y.Ubicacion.X));

            CreateMap<SalaDeCineDTO, SalaDeCine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<SalaDeCineCreacionDTO, SalaDeCine>()
                 .ForMember(x => x.Ubicacion, x => x.MapFrom(y =>
                geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));


            CreateMap<IdentityUser, UsuarioDTO>();

            CreateMap<Review, ReviewDTO>()
            .ForMember(x => x.NombreUsuario, x => x.MapFrom(y => y.Usuario.UserName));

            CreateMap<ReviewDTO, Review>();
            CreateMap<ReviewCreacionDTO, Review>();

            CreateMap<LlaveAPI, SuscripcionLlaveDTO>();
            CreateMap<RestriccionDominio, RestriccionDominioDTO>();
            CreateMap<RestriccionIP, RestriccionIPDTO>();
        }

        private List<PeliculaGenero> MapPeliculasGenerosCreacion(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculaGenero>();
            if (peliculaCreacionDTO.GenerosIds == null) { return resultado; }
            foreach (var id in peliculaCreacionDTO.GenerosIds)
            {
                resultado.Add(new PeliculaGenero() { GeneroId = id });
            }
            return resultado;
        }

        private List<PeliculaActor> MapPeliculasActoresCreacion(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculaActor>();
            if (peliculaCreacionDTO.Actores == null) { return resultado; }

            foreach (var actor in peliculaCreacionDTO.Actores)
            {
                resultado.Add(new PeliculaActor() { ActorId = actor.ActorId, Personaje = actor.Personaje });
            }

            return resultado;
        }

        private List<PeliculaGenero> MapPeliculasGenerosActualizacion(PeliculaActualizacionDTO peliculaActualizacionDTO, Pelicula pelicula) 
        {
            var resultado = new List<PeliculaGenero>();
            if (peliculaActualizacionDTO.GenerosIds == null) { return resultado; }
            foreach (var id in peliculaActualizacionDTO.GenerosIds)
            {
                resultado.Add(new PeliculaGenero() { GeneroId = id });
            }
            return resultado;
        }

        private List<PeliculaActor> MapPeliculasActoresActualizacion(PeliculaActualizacionDTO peliculaActualizacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculaActor>();
            if (peliculaActualizacionDTO.Actores == null) { return resultado; }

            foreach (var actor in peliculaActualizacionDTO.Actores)
            {
                resultado.Add(new PeliculaActor() { ActorId = actor.ActorId, Personaje = actor.Personaje });
            }

            return resultado;
        }

        private List<GeneroDTO> MapPeliculasGeneros(Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<GeneroDTO>();
            if (pelicula.PeliculasGeneros == null) { return resultado; }
            foreach (var generoPelicula in pelicula.PeliculasGeneros)
            {
                resultado.Add(new GeneroDTO() { Id = generoPelicula.GeneroId, Nombre = generoPelicula.Genero.Nombre });
            }

            return resultado;
        }


        private List<ActorPeliculaDetalleDTO> MapPeliculasActores(Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<ActorPeliculaDetalleDTO>();
            if (pelicula.PeliculasActores == null) { return resultado; }
            foreach (var actorPelicula in pelicula.PeliculasActores)
            {
                resultado.Add(new ActorPeliculaDetalleDTO
                {
                    ActorId = actorPelicula.ActorId,
                    Personaje = actorPelicula.Personaje,
                    NombrePersona = actorPelicula.Actor.Nombre
                });
            }

            return resultado;
        }
    }
}
