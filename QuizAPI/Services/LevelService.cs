using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Context;
using QuizAPI.DTOs;
using QuizAPI.Helpers;
using QuizAPI.Models;
using System.Net;

namespace QuizAPI.Services
{
    public class LevelService
    {
        private readonly QuizDbContext _context;
        private readonly IMapper _mapper;

        public LevelService(QuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<LevelDTO>> Find(int id)
        {
            var level = await _context.Levels.FirstOrDefaultAsync(l=> l.Id == id);

            if(level != null)
            {
                var levelDTO = _mapper.Map<LevelDTO>(level);
                return Result.Ok(levelDTO);
            }
            else
            {
                return Result.Error("Não foi Possivel encontrar o Nivel!");
            }

        }
        public async Task<Result<IEnumerable<LevelDTO>>> GetList()
        {
            var levels = await _context.Levels.ToListAsync();
            
            if(levels != null)
            {
                var LevelsDTO = _mapper.Map<IEnumerable<LevelDTO>>(levels);
                
                return Result.Ok(LevelsDTO);
            }
            else
            {
                return Result.Error("Não foi Possivel encontrar o Nivel!");
            }

        }

        public async Task<Result<bool>> Create(LevelDTO levelDTO)
        {
            var level = _mapper.Map<Level>(levelDTO);

            _context.Levels.Add(level);

            var res = await _context.SaveChangesAsync() > 0;

            if (!res) return Result.Error("Não foi possivel Criar Nivel!");

            return Result.Ok(res).WithStatusCode(HttpStatusCode.Created);

        }

        public async Task<Result<bool>> Update(LevelDTO levelDTO)
        {
            var level = _mapper.Map<Level>(levelDTO);
            _context.Levels.Update(level);
            _context.Entry(level).State = EntityState.Modified;
            var res = await  _context.SaveChangesAsync() > 0;

            if (!res) return Result.Error("Não foi possivel atualizar Nivel!");

            return Result.Ok(res).WithStatusCode(HttpStatusCode.OK);

        }

        public async Task<Result<bool>> Delete(int id)
        {

            var level = await _context.Levels.FirstOrDefaultAsync(l => l.Id == id);

            if (level != null)
            {
                _context.Levels.Remove(level);

                var res = await _context.SaveChangesAsync() > 0;
                if (!res) return Result.Error("Não foi possivel excluir Nivel!");

                return Result.Ok(res).WithStatusCode(HttpStatusCode.OK);
            }
            else
            {
                return Result.Error("Não foi possivel excluir Pergunta!");
            }

        }
    }
}
