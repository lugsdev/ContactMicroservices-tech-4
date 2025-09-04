using Microsoft.AspNetCore.Mvc;
using ContactModels;
using ContactModels.DTOs;
using Common.Messaging;
using Common.Events;

namespace ContactUpdate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IMessagePublisher _messagePublisher;

        public ContactController(ILogger<ContactController> logger, IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ContactResponseDto>> UpdateContact(int id, [FromBody] UpdateContactDto updateContactDto)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização de contato ID: {Id}", id);

                if (id <= 0)
                {
                    return BadRequest("ID deve ser maior que zero");
                }

                if (id != updateContactDto.Id)
                {
                    return BadRequest("ID da URL deve corresponder ao ID do corpo da requisição");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // TODO: Verificar se o contato existe no banco de dados via Dapper
                // Por enquanto, simular que o contato existe
                var existingContact = new Contact
                {
                    Id = id,
                    Nome = "Nome Antigo",
                    DDD = "11",
                    NumeroCelular = "987654321",
                    Email = "antigo@email.com",
                    DataCriacao = DateTime.UtcNow.AddDays(-10)
                };

                if (existingContact == null)
                {
                    _logger.LogWarning("Contato com ID {Id} não encontrado para atualização", id);
                    return NotFound($"Contato com ID {id} não encontrado");
                }

                // Atualizar os dados
                existingContact.Nome = updateContactDto.Nome;
                existingContact.DDD = updateContactDto.DDD;
                existingContact.NumeroCelular = updateContactDto.NumeroCelular;
                existingContact.Email = updateContactDto.Email;
                existingContact.DataAtualizacao = DateTime.UtcNow;

                // TODO: Salvar no banco de dados via Dapper
                // Publicar evento no RabbitMQ
                var contactUpdatedEvent = new ContactUpdatedEvent
                {
                    ContactId = existingContact.Id,
                    Nome = existingContact.Nome,
                    DDD = existingContact.DDD,
                    NumeroCelular = existingContact.NumeroCelular,
                    Email = existingContact.Email,
                    DataAtualizacao = existingContact.DataAtualizacao
                };

                await _messagePublisher.PublishAsync(contactUpdatedEvent);
                _logger.LogInformation("Evento ContactUpdated publicado para contato ID: {Id}", id);

                _logger.LogInformation("Contato atualizado com sucesso. ID: {Id}", id);

                var response = new ContactResponseDto
                {
                    Id = existingContact.Id,
                    Nome = existingContact.Nome,
                    DDD = existingContact.DDD,
                    NumeroCelular = existingContact.NumeroCelular,
                    Email = existingContact.Email,
                    DataCriacao = existingContact.DataCriacao,
                    DataAtualizacao = existingContact.DataAtualizacao
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar contato com ID {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ContactResponseDto>> PartialUpdateContact(int id, [FromBody] Dictionary<string, object> updates)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização parcial de contato ID: {Id}", id);

                if (id <= 0)
                {
                    return BadRequest("ID deve ser maior que zero");
                }

                // TODO: Verificar se o contato existe no banco de dados via Dapper
                var existingContact = new Contact
                {
                    Id = id,
                    Nome = "Nome Atual",
                    DDD = "11",
                    NumeroCelular = "987654321",
                    Email = "atual@email.com",
                    DataCriacao = DateTime.UtcNow.AddDays(-10)
                };

                if (existingContact == null)
                {
                    _logger.LogWarning("Contato com ID {Id} não encontrado para atualização parcial", id);
                    return NotFound($"Contato com ID {id} não encontrado");
                }

                // Aplicar atualizações parciais
                foreach (var update in updates)
                {
                    switch (update.Key.ToLower())
                    {
                        case "nome":
                            existingContact.Nome = update.Value?.ToString() ?? existingContact.Nome;
                            break;
                        case "ddd":
                            existingContact.DDD = update.Value?.ToString() ?? existingContact.DDD;
                            break;
                        case "numerocelular":
                            existingContact.NumeroCelular = update.Value?.ToString() ?? existingContact.NumeroCelular;
                            break;
                        case "email":
                            existingContact.Email = update.Value?.ToString() ?? existingContact.Email;
                            break;
                    }
                }

                existingContact.DataAtualizacao = DateTime.UtcNow;

                // TODO: Validar dados atualizados
                // TODO: Salvar no banco de dados via Dapper
                // TODO: Publicar evento no RabbitMQ

                _logger.LogInformation("Contato atualizado parcialmente com sucesso. ID: {Id}", id);

                var response = new ContactResponseDto
                {
                    Id = existingContact.Id,
                    Nome = existingContact.Nome,
                    DDD = existingContact.DDD,
                    NumeroCelular = existingContact.NumeroCelular,
                    Email = existingContact.Email,
                    DataCriacao = existingContact.DataCriacao,
                    DataAtualizacao = existingContact.DataAtualizacao
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar parcialmente contato com ID {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Service = "ContactUpdate.API", Timestamp = DateTime.UtcNow });
        }
    }
}

