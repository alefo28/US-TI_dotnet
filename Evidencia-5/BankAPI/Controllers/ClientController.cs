using BankAPI.Data.BankModels;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers;

[Authorize(Policy = "SuperAdmin")]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly ClientService _service;

    public ClientController(ClientService service)
    {
        _service = service;
    }

    [HttpGet("getall")]
    public async Task<IEnumerable<Client>> Get()
    {
        return await _service.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await _service.GetById(id);

        if (client is null)
        {
            return ClientNoFound(id);
        }
        return client;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(Client client)
    {
        var newClient = await _service.Create(client);

        return CreatedAtAction(nameof(GetById), new { id = newClient.Id }, client);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, Client client)
    {
        if (id != client.Id)
        {
            return BadRequest(
                new
                {
                    message = $"El Id({id}) de la URL no coincide con el ID({client.Id}) de cuerpo de la solicitud:"
                }
            );
        }

        var clienToUpdate = await _service.GetById(id);

        if (clienToUpdate is not null)
        {
            await _service.Update(id, client);
            return NoContent();
        }
        else
        {
            return ClientNoFound(id);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var clienToDelete = await _service.GetById(id);

        if (clienToDelete is not null)
        {
            await _service.Delete(id);
            return Ok();
        }
        else
        {
            return ClientNoFound(id);
        }
    }

    public NotFoundObjectResult ClientNoFound(int id)
    {
        return NotFound(new { message = $"El cliente con ID = {id} no existe." });
    }
}
