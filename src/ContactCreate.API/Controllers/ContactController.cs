using Microsoft.AspNetCore.Mvc;
using ContactModels;
using ContactModels.DTOs;
using Common.Messaging;
using Common.Events;
using Infrastructure.Repositories;

namespace ContactCreate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IContactRepository _contactRepository;

        public ContactController(
            ILogger<ContactController> logger, 
            IMessagePublisher messagePublisher,
            IContactRepository contactRepository)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
            _contactRepository = contactRepository;
        }

        [HttpPost]
        public async Task<ActionResult<ContactResponseDto>> CreateContact([FromBody] CreateContactDto createContactDto)
        {
            try
            {
                _logger.LogInformation("Iniciando criação de contato para {Nome}", createContactDto.Nome);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar se já existe contato com o mesmo email
                var existingContact = await _contactRepository.GetByEmailAsync(createContactDto.Email);
                if (existingContact != null)
                {
                    return Conflict($"Já existe um contato com o email {createContactDto.Email}");
                }

                // Criar o contato
                var contact = new Contact
                {
                    Nome = createContactDto.Nome,
                    DDD = createContactDto.DDD,
                    NumeroCelular = createContactDto.NumeroCelular,
                    Email = createContactDto.Email,
                    DataCriacao = DateTime.UtcNow
                };

                // Salvar no banco de dados via Dapper
                var createdContact = await _contactRepository.CreateAsync(contact);

                // Publicar evento no RabbitMQ
                var contactCreatedEvent = new ContactCreatedEvent
                {
                    ContactId = createdContact.Id,
                    Nome = createdContact.Nome,
                    DDD = createdContact.DDD,
                    NumeroCelular = createdContact.NumeroCelular,
                    Email = createdContact.Email
                };

                await _messagePublisher.PublishAsync(contactCreatedEvent);
                _logger.LogInformation("Evento ContactCreated publicado para contato ID: {Id}", createdContact.Id);
                _logger.LogInformation("Contato criado com sucesso. ID: {Id}", createdContact.Id);

                // Retornar resposta
                var response = new ContactResponseDto
                {
                    Id = createdContact.Id,
                    Nome = createdContact.Nome,
                    DDD = createdContact.DDD,
                    NumeroCelular = createdContact.NumeroCelular,
                    Email = createdContact.Email,
                    DataCriacao = createdContact.DataCriacao,
                    DataAtualizacao = createdContact.DataAtualizacao
                };

                return CreatedAtAction(nameof(CreateContact), new { id = createdContact.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar contato");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Service = "ContactCreate.API", Timestamp = DateTime.UtcNow });
        }
    }
}

