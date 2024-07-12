using Microsoft.AspNetCore.Mvc;
using QuizAPI.DTOs;
using QuizAPI.Models;
using QuizAPI.Services;

namespace QuizAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnswereController :  BaseController
    {
        private readonly AnswereService _answereService;

        public AnswereController(AnswereService answereService)
        {
            _answereService = answereService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Find(int id)
        {
            var answerDTO = await  _answereService.Find(id);

            return Resolve(answerDTO);
        }

        [HttpGet("GetWithQuestion/{id}")]
        public async Task<IActionResult> GetWithQuestion(int id)
        {
            var answerDTO = await _answereService.GetWithQuestion(id);

            return Resolve(answerDTO);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var answersDTO = await _answereService.GetList();

            return Resolve(answersDTO);
        }

        [HttpPost]

        public async Task<IActionResult> Create(AnswerDTO answerDTO)
        {
            var res = await _answereService.Create(answerDTO);

            return Resolve(res);
        }

        [HttpPut]
        public async Task<IActionResult> Update(AnswerDTO answerDTO)
        {
            var res = await _answereService.Update(answerDTO);

            return Resolve(res);
        }

        [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete(int id)
        {
            var res = await _answereService.Delete(id);

            return Resolve(res);
        }
    }
}
