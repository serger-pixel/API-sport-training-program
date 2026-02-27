using API_sprot_training_program.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Reflection;

namespace TrainingProgramApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramsController : ControllerBase
    {
        private static List<TrainingProgram> _programs = new()
        {
            new TrainingProgram
            {
                Id = 1,
                Title = "Начальная программа",
                Description = "Программа для новичков",
                DurationInWeek = 6,
                CntInWeek = 10
            }
        };

        private static int _currentId = 2;

    
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _programs.Select(p => MapToDto(p));
            return Ok(result);
        }

        
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var program = _programs.FirstOrDefault(element => element.Id == id);

            if (program == null)
                return NotFound();

            return Ok(MapToDto(program));
        }

        
        [HttpPost]
        public IActionResult Create(DtoCreateUpdate dto)
        {
            var program = new TrainingProgram
            {
                Id = _currentId++,
                Title = dto.Title,
                Description = dto.Description,
                DurationInWeek= dto.DurationInWeek,
                
            };

            _programs.Add(program);
            return CreatedAtAction(nameof(GetById), 
                new { id = program.Id },
                MapToDto(program));
        }

        
        [HttpPut("{id}")]
        public IActionResult Update(int id, DtoCreateUpdate dto)
        {
            var program = _programs.FirstOrDefault(element => element.Id == id);
            if (program == null)
                return NotFound();

            program.Title = dto.Title;
            program.Description = dto.Description;
            program.CntInWeek = dto.CntInWeek;
            program.DurationInWeek = dto.DurationInWeek;

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var program = _programs.FirstOrDefault(element => element.Id == id);

            if (program == null)
                return NotFound();

            _programs.Remove(program);

            return NoContent();
        }

        
        private static DtoRead MapToDto(TrainingProgram program)
        {
            return new DtoRead
            {
                Id = program.Id,
                Title = program.Title,
                Description = program.Description,
                DurationInWeek = program.DurationInWeek,
                CntInWeek = program.CntInWeek
            };
        }
    }
}
