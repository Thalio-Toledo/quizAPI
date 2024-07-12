using Microsoft.AspNetCore.Mvc;
using QuizAPI.DTOs;
using QuizAPI.Filter;
using QuizAPI.Helpers;
using QuizAPI.Models;
using QuizAPI.Services;
using System.Net;

namespace QuizAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionController : BaseController
    {
        private readonly QuestionService _questionService;

        public QuestionController(QuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Find(int id)
        {
            var questionDTO = await _questionService.Find(id);

            return Resolve(questionDTO);
        }

        [HttpGet("FindByLevel/{id}")]
        public async Task<IActionResult> FindByLevel(int id)
        {
            var questionDTO = await _questionService.FindByLevel(id);

            return Resolve(questionDTO);
        }

        [ProducesResponseType<Result<bool>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<Result.ResultError>((int)HttpStatusCode.InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var questionsDTO = await _questionService.GetList();

            return Resolve(questionsDTO);
        }

        [HttpGet("GetByLevel/{idLevel}")]
        public async Task<IActionResult> GetByLevel(int idLevel)
        {
            var res = await _questionService.GetByLevel(idLevel);

            return Resolve(res);
        }

        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPaginated([FromQuery] QuestionFilter questionFilter)
        {
            var res = await _questionService.GetListPaginated(questionFilter);

            return Resolve(res);
        }

        [ProducesResponseType<Result<bool>>((int)HttpStatusCode.Created)]
        [ProducesResponseType<Result.ResultError>((int) HttpStatusCode.InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(QuestionDTO questionDTO)
        {
            var res = await _questionService.Create(questionDTO);

            return Resolve(res);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Question question)
        {
            var res  = await _questionService.Update(question);

            return Resolve(res);
        }

        [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete(int id)
        {
            var res = await  _questionService.Delete(id);


            return Resolve(res);
        }

      
    }
}
