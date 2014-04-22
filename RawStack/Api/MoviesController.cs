﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Raven.Client;
using RawStack.Models;

namespace RawStack.Api
{
    public class MoviesController : ApiController
    {
        private readonly IDocumentSession _session;

        public MoviesController(IDocumentSession session)
        {
            _session = session;
        }

        public class MoviesRequest
        {
            public int Page { get; set; }
            public string[] Genres { get; set; }
            public string Director { get; set; }
        }

        public IEnumerable<Movie> GetMovies([FromUri]MoviesRequest request)
        //public IEnumerable<Movie> GetMovies(
        //    int page, [FromUri]string[] genres, [FromUri]string director)
        {
            const int pageSize = 32;

            var query = _session.Advanced.LuceneQuery<Movie>();
            if (request.Genres != null && request.Genres.Length > 0 && request.Genres[0] != null)
            {
                var filter = "Genres:(\"" + string.Join("\" AND \"", request.Genres) + "\")";
                query = query.Where(filter);
            }

            if (request.Director != null)
            {
                var filter = "AbridgedDirectors:(\"" + request.Director + "\")";
                query = query.Where(filter);
            }

            var movies = query
                .OrderBy(m => m.Title)
                .Skip(request.Page * pageSize)
                .Take(pageSize)
                .ToList();

            return movies;
        }

        public Movie GetMovie(int id)
        {
            var movie = _session.Load<Movie>(id);

            return movie;
        }

        public void PostMovie(Movie movie)
        {
            _session.Store(movie);
            _session.SaveChanges();
        }

        public void DeleteMovie(int id)
        {
            var movie = _session.Load<Movie>(id);
            _session.Delete(movie);
            _session.SaveChanges();
        }
    }
}