using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Context;
using QuizAPI.DTOs;
using QuizAPI.Helpers;
using QuizAPI.Models;
using System.Net;

namespace QuizAPI.Services
{
    public class AnswereService
    {
        private readonly QuizDbContext _context;
        private readonly IMapper _mapper;

        public AnswereService(QuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<AnswerDTO>> Find(int id)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(a => a.Id == id);

            if(answer != null)
            {
                var answerDTO = _mapper.Map<AnswerDTO>(answer);
                return Result.Ok(answerDTO);
            }
            else
            {
                return Result.Error("Não foi Possivel encontrar Resposta!");
            }

        }

        public async Task<Result<AnswerDTO>> GetWithQuestion(int id)
        {
            var answer = await _context.Answers
                .Include(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (answer != null)
            {
                var answerDTO = _mapper.Map<AnswerDTO>(answer);
                return Result.Ok(answerDTO);
            }
            else
            {
                return Result.Error("Não foi Possivel encontrar Resposta!");
            }

        }
        public async Task<Result<IEnumerable<AnswerDTO>>> GetList()
        {
            var answers = await _context.Answers.ToListAsync();

            if(answers != null)
            {
                var answersDTO = _mapper.Map<IEnumerable<AnswerDTO>>(answers);
                return Result.Ok(answersDTO);
            }
            else
            {
                return Result.Error("Não foi Possivel encontrar Respostas!");
            }

        }

        public async Task<Result<bool>> Create(AnswerDTO answerDTO)
        {
            var answere = _mapper.Map<Answer>(answerDTO);

            _context.Add(answere);
            var res = await _context.SaveChangesAsync() > 0;

            if(!res) return Result.Error("Não foi Possivel criar Resposta!");

            return Result.Ok(res);
        }

        public async Task<Result<bool>> Update(AnswerDTO answerDTO)
        {
            var answere = _mapper.Map<Answer>(answerDTO);

            _context.Answers.Update(answere);

           var res = await  _context.SaveChangesAsync() > 0;

            if (!res) return Result.Error("Não foi possivel alterar a Resposta!");

            return Result.Ok(res);
        }

        public async Task<Result<bool>> Delete(int id)
        {
           
            var answer = await  _context.Answers.FirstOrDefaultAsync(a=> a.Id == id);

            if (answer != null)
            {
                _context.Remove(answer);

                var res = await _context.SaveChangesAsync() > 0;
                if (!res) return Result.Error("Não foi possivel excluir Resposta!");

                return Result.Ok(res).WithStatusCode(HttpStatusCode.OK);
            }
            else
            {
                return Result.Error("Não foi possivel excluir Pergunta!");
            }
        }
            
        
    }
}
