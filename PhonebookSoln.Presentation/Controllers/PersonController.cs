using Microsoft.AspNetCore.Mvc;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPersons()
        {
            var persons = await _personService.GetAllPersonsAsync();
            return Ok(persons);
        }

        [HttpGet("{personId}")]
        public async Task<IActionResult> GetPersonById(Guid personId)
        {
            var person = await _personService.GetPersonByIdAsync(personId);

            if (person == null)
            {
                return NotFound($"Person with ID {personId} not found.");
            }

            return Ok(person);
        }

        [HttpPost]
        public async Task<IActionResult> AddPerson([FromBody] PersonDto personDto)
        {
            await _personService.AddPersonAsync(personDto);
            return CreatedAtAction(nameof(GetPersonById), new { personId = personDto.Id }, personDto);
        }

        [HttpPut("{personId}")]
        public async Task<IActionResult> UpdatePerson(Guid personId, [FromBody] PersonDto personDto)
        {
            if (personId != personDto.Id)
            {
                return BadRequest("Person ID mismatch.");
            }

            await _personService.UpdatePersonAsync(personDto);
            return NoContent();
        }

        [HttpDelete("{personId}")]
        public async Task<IActionResult> DeletePerson(Guid personId)
        {
            await _personService.DeletePersonAsync(personId);
            return NoContent();
        }
    }
}