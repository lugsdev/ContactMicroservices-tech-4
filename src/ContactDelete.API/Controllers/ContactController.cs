using Microsoft.AspNetCore.Mvc;
using ContactModels;
using ContactModels.DTOs;
using Common.Messaging;
using Common.Events;

namespace ContactDelete.API.Controllers
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteContact(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão de contato ID: {Id}", id);

                if (id <= 0)
                {
                    return BadRequest("ID deve ser maior que zero");
                }

                // TODO: Verificar se o contato existe no banco de dados via Dapper
                var existingContact = new Contact
                {
                    Id = id,
                    Nome = "Contato a ser excluído",
                    DDD = "11",
                    NumeroCelular = "987654321",
                    Email = "contato@email.com",
                    DataCriacao = DateTime.UtcNow.AddDays(-10)
                };

                if (existingContact == null)
                {
                    _logger.LogWarning("Contato com ID {Id} não encontrado para exclusão", id);
                    return NotFound($"Contato com ID {id} não encontrado");
                }

                // TODO: Excluir do banco de dados via Dapper
                // Publicar evento no RabbitMQ
                var contactDeletedEvent = new ContactDeletedEvent
                {
                    ContactId = existingContact.Id,
                    Nome = existingContact.Nome
                };

                await _messagePublisher.PublishAsync(contactDeletedEvent);
                _logger.LogInformation("Evento ContactDeleted publicado para contato ID: {Id}", id);

                _logger.LogInformation("Contato excluído com sucesso. ID: {Id}, Nome: {Nome}", 
                    id, existingContact.Nome);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir contato com ID {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpDelete("batch")]
        public async Task<ActionResult> DeleteMultipleContacts([FromBody] List<int> ids)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão em lote de {Count} contatos", ids.Count);

                if (ids == null || !ids.Any())
                {
                    return BadRequest("Lista de IDs não pode estar vazia");
                }

                if (ids.Any(id => id <= 0))
                {
                    return BadRequest("Todos os IDs devem ser maiores que zero");
                }

                var deletedCount = 0;
                var notFoundIds = new List<int>();

                foreach (var id in ids)
                {
                    // TODO: Verificar se o contato existe no banco de dados via Dapper
                    var existingContact = new Contact
                    {
                        Id = id,
                        Nome = $"Contato {id}",
                        DDD = "11",
                        NumeroCelular = "987654321",
                        Email = $"contato{id}@email.com",
                        DataCriacao = DateTime.UtcNow.AddDays(-10)
                    };

                    if (existingContact != null)
                    {
                        // TODO: Excluir do banco de dados via Dapper
                        // TODO: Publicar evento no RabbitMQ
                        deletedCount++;
                        _logger.LogInformation("Contato excluído: ID {Id}", id);
                    }
                    else
                    {
                        notFoundIds.Add(id);
                        _logger.LogWarning("Contato não encontrado: ID {Id}", id);
                    }
                }

                var result = new
                {
                    DeletedCount = deletedCount,
                    NotFoundIds = notFoundIds,
                    Message = $"{deletedCount} contato(s) excluído(s) com sucesso"
                };

                _logger.LogInformation("Exclusão em lote concluída. Excluídos: {DeletedCount}, Não encontrados: {NotFoundCount}", 
                    deletedCount, notFoundIds.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir contatos em lote");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Service = "ContactDelete.API", Timestamp = DateTime.UtcNow });
        }
    }
}

