using Microsoft.AspNetCore.Mvc;
using QuizAPI.DTOs;
using QuizAPI.Models;
using QuizAPI.Services;

namespace QuizAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LevelController : BaseController
    {
        private readonly LevelService _LevelService;

        public LevelController(LevelService service)
        {
            _LevelService = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Find(int id)
        {
            var level = await _LevelService.Find(id);

            return Resolve(level);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var levels = await _LevelService.GetList();

            return Resolve(levels);
        }

        [HttpPost]

        public async Task<IActionResult> Create(LevelDTO levelDTO)
        {
            var levelCreated = await _LevelService.Create(levelDTO);

            return Resolve(levelCreated);
        }

        [HttpPut]
        public async Task<IActionResult> Update(LevelDTO levelDTO)
        {
            var levelAltered = await _LevelService.Update(levelDTO);

            return Resolve(levelAltered);
        }

        [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete(int id)
        {
            var levelDeleted = await _LevelService.Delete(id);

            return Resolve(levelDeleted);
        }
    }
}
