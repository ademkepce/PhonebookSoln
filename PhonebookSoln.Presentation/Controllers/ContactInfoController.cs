using Microsoft.AspNetCore.Mvc;
using PhonebookSoln.Application.Interfaces;
using PhonebookSoln.Core.Dtos;

namespace PhonebookSoln.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactInfoController : ControllerBase
    {
        private readonly IContactInfoService _contactInfoService;

        public ContactInfoController(IContactInfoService contactInfoService)
        {
            _contactInfoService = contactInfoService;
        }

        [HttpGet("person/{personId}")]
        public async Task<IActionResult> GetContactInfosByPersonId(Guid personId)
        {
            var contactInfos = await _contactInfoService.GetContactInfosByPersonIdAsync(personId);
            return Ok(contactInfos);
        }

        [HttpGet("{contactInfoId}")]
        public async Task<IActionResult> GetContactInfoById(Guid contactInfoId)
        {
            var contactInfo = await _contactInfoService.GetContactInfoByIdAsync(contactInfoId);

            if (contactInfo == null)
            {
                return NotFound($"Contact Info with ID {contactInfoId} not found.");
            }

            return Ok(contactInfo);
        }

        [HttpPost]
        public async Task<IActionResult> AddContactInfo([FromBody] ContactInfoDto contactInfoDto)
        {
            await _contactInfoService.AddContactInfoAsync(contactInfoDto);
            return CreatedAtAction(nameof(GetContactInfoById), new { contactInfoId = contactInfoDto.Id }, contactInfoDto);
        }

        [HttpPut("{contactInfoId}")]
        public async Task<IActionResult> UpdateContactInfo(Guid contactInfoId, [FromBody] ContactInfoDto contactInfoDto)
        {
            if (contactInfoId != contactInfoDto.Id)
            {
                return BadRequest("Contact Info ID mismatch.");
            }

            await _contactInfoService.UpdateContactInfoAsync(contactInfoDto);
            return NoContent();
        }

        [HttpDelete("{contactInfoId}")]
        public async Task<IActionResult> DeleteContactInfo(Guid contactInfoId)
        {
            await _contactInfoService.DeleteContactInfoAsync(contactInfoId);
            return NoContent();
        }
    }
}