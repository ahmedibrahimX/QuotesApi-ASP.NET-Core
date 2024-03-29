﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using QuotesApi.Data;
using QuotesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QuotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotesController : ControllerBase
    {
        private QuotesDbContext _quotesDbContext;

        public QuotesController(QuotesDbContext quotesDbContext)
        {
            _quotesDbContext = quotesDbContext;
        }

        // GET: api/<QuotesController>
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [AllowAnonymous]
        public IActionResult Get(string sort, int? pageNumber, int? pageSize)
        {
            IQueryable<Quote> quotes;
            int currentPageNumber = pageNumber ?? 1;
            int currentPageSize = pageSize ?? 5;
            switch (sort)
            {
                case "desc":
                    quotes = _quotesDbContext.Quotes.OrderByDescending(q => q.CreatedAt);
                    break;
                case "asc":
                    quotes = _quotesDbContext.Quotes.OrderBy(q => q.CreatedAt);
                    break;
                default:
                    quotes = _quotesDbContext.Quotes;
                    break;
            }
            return Ok(quotes.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
        }

        // GET api/Quotes/SearchQuote?type=type2
        [HttpGet]
        [Route("[action]")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public IActionResult SearchQuote(string type)
        {
            var quotes = _quotesDbContext.Quotes.Where(q => q.Type.StartsWith(type));
            return Ok(quotes);
        }

        // GET api/<QuotesController>/5
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public IActionResult Get(int id)
        {
            var quote = _quotesDbContext.Quotes.Find(id);
            if(quote == null)
            {
                return NotFound("Record Not Found");
            }
            return Ok(quote);
        }

        [HttpGet("[action]")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public IActionResult MyQuote()
        {
            string userid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var quotes = _quotesDbContext.Quotes.Where(quote => quote.UserId == userid);
            return Ok(quotes);
        }

        // POST api/<QuotesController>
        [HttpPost]
        public IActionResult Post([FromBody] Quote quote)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            quote.UserId = userId;
            _quotesDbContext.Quotes.Add(quote);
            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        // PUT api/<QuotesController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Quote quote)
        {
            string userid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var entity = _quotesDbContext.Quotes.Find(id);
            if(entity==null)
            {
                return NotFound("Record Not Found");
            }
            if(userid != entity.UserId)
            {
                return BadRequest("Unauthorized To Update Record");
            }
            entity.Title = quote.Title;
            entity.Author = quote.Author;
            entity.Description = quote.Description;
            entity.Type = quote.Type;
            entity.CreatedAt = quote.CreatedAt;
            _quotesDbContext.SaveChanges();
            return Ok("Record Updated Successfully");
        }

        // DELETE api/<QuotesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string userid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var quote = _quotesDbContext.Quotes.Find(id);
            if(quote==null)
            {
                return NotFound("Record Not Found");
            }
            if (userid != quote.UserId)
            {
                return BadRequest("Unauthorized To Update Record");
            }
            _quotesDbContext.Quotes.Remove(quote);
            _quotesDbContext.SaveChanges();
            return Ok("Quote Deleted Successfully");
        }
    }
}
