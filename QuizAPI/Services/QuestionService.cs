using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Context;
using QuizAPI.DTOs;
using QuizAPI.Filter;
using QuizAPI.Helpers;
using QuizAPI.Models;
using System.Net;

namespace QuizAPI.Services
{
    public class QuestionService
    {
        private readonly QuizDbContext _context;
        private readonly IMapper _mapper;

        public QuestionService(QuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<QuestionDTO>> Find(int id)
        {
           var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(question => question.Id == id)
                ;

           if (question == null) return Result.Error("Não foi possivel encontrar Pergunta!");

           var questionsDTO = _mapper.Map<QuestionDTO>(question);

           return Result.Ok(questionsDTO);
        }

        public async Task<Result<QuestionDTO>> FindByLevel(int idLevel)
        {
            var questions = await _context.Questions
                .Where(q => q.IdLevel == idLevel)
                .Include(q => q.Answers)
                .ToListAsync();

       
            var random = new Random();

            var randomIndex = random.Next(questions.Count);

            var question = questions[randomIndex];

            if (question == null) return Result.Error("Não foi possivel encontrar Pergunta!");

            var questionsDTO = _mapper.Map<QuestionDTO>(question);

            return Result.Ok(questionsDTO);
           
        }


        public async Task<Result<IEnumerable<QuestionDTO>>> GetList()
        {
            var res = await _context.Questions.Include(q => q.Answers).ToListAsync();

            var questionsDTO = _mapper.Map<IEnumerable<QuestionDTO>>(res);


            return Result.Ok(questionsDTO);
        }

        public async Task<Result<IEnumerable<QuestionDTO>>> GetByLevel(int idLevel)
        {
            var res = await _context.Questions
                .Where(q => q.IdLevel == idLevel)
                .Include(q => q.Answers)
                .ToListAsync();

            var questionsDTO = _mapper.Map<IEnumerable<QuestionDTO>>(res);

            return Result.Ok(questionsDTO);

            
        }

        public async Task<Result<IEnumerable<QuestionDTO>>> GetListPaginated(QuestionFilter questionFilter)
        {
            var questions = await _context.Questions
                .Include(q=>q.Answers)
                .ToListAsync();

            if(questionFilter.IdLevel != null)
            {
                questions = questions.Where(q => q.IdLevel == questionFilter.IdLevel).ToList();
            }

            questions = questions.Skip((questionFilter.PageNumber - 1) * questionFilter.PageSize).Take(questionFilter.PageSize).ToList();

            var questionsDTO = _mapper.Map<IEnumerable<QuestionDTO>>(questions);

            return Result.Ok(questionsDTO);
        }

        public async Task<Result<bool>> Create(QuestionDTO questionDTO)
        {
            var question = _mapper.Map<Question>(questionDTO);

            _context.Add(question);
            var res = await _context.SaveChangesAsync() > 0;

            if (!res) return Result.Error("Não foi possivel Criar Pergunta!");
                
            return Result.Ok(res).WithStatusCode(HttpStatusCode.Created);

        }


        public async Task<Result<bool>> Update(Question question) 
        {
            _context.Questions.Update(question);
            _context.Entry(question).State = EntityState.Modified;
            var res = await  _context.SaveChangesAsync() > 0;

            if (!res) return Result.Error("Não foi possivel Atualizar Pergunta!");

            return Result.Ok(res).WithStatusCode(HttpStatusCode.OK);

        }

        public async Task<Result<bool>> Delete(int id) 
        {
            var question = await _context.Questions.FirstOrDefaultAsync(question => question.Id == id);

            if (question != null)
            {
                _context.Questions.Remove(question);

                var res = await _context.SaveChangesAsync() > 0;
                if (!res) return Result.Error("Não foi possivel excluir Pergunta!");

                return Result.Ok(res).WithStatusCode(HttpStatusCode.OK);
            }
            else
            {
                return Result.Error("Não foi possivel excluir Pergunta!");
            }



        }
    }
}
