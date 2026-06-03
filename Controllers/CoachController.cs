using API_sprot_training_program.Models;
using API_sprot_training_program.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Reflection;

namespace TrainingcoachApi.Controllers
{
    [Route("coaches")]
    [ApiController]
    public class CoachController : ControllerBase
    {
        CoachService _service;
        public CoachController(CoachService service)
        {
            _service = service;
        }


        [HttpGet]
        [Authorize]
        public async Task<List<CoachOutput>> Get() => await _service.GetAllAsync();


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(String id)
        {
            var coach = await _service.GetByIdAsync(id);

            if (coach is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(Get), coach);
        }

        [HttpGet("filter")]
        [Authorize]
        public async Task<IActionResult> GetByFilter(String nameProperty, String value)
        {

            var coachs = await _service.GetByFilter(nameProperty, value);
            if (coachs is null)
            {
                return NotFound();
            }
            return CreatedAtAction(nameof(GetByFilter), coachs);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(CoachInput coach)
        {
            await _service.CreateAsync(coach);
            return CreatedAtAction(nameof(Post), coach);
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(String id, CoachInput updatecoach)
        {
            var currentcoach = await _service.GetByIdAsync(id);

            if (currentcoach is null)
            {
                return NotFound();
            }

            await _service.UpdateAsync(id, updatecoach);

            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(String id)
        {
            var result = await _service.DeleteAsync(id);

            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpDelete("all")]
        [Authorize]
        public async Task<IActionResult> DeleteAll()
        {
            await _service.DeleteAllAsync();
            return NoContent();
        }
    }
}
