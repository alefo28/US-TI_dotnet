using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountService accountservice;
    private readonly AccountTypeService accountTypeService;
    private readonly ClientService clientService;

    public AccountController(
        AccountService accountservice,
        AccountTypeService accountTypeService,
        ClientService clientService
    )
    {
        this.accountservice = accountservice;
        this.accountTypeService = accountTypeService;
        this.clientService = clientService;
    }

    [HttpGet]
    public async Task<IEnumerable<Account>> Get()
    {
        return await accountservice.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetById(int id)
    {
        var account = await accountservice.GetById(id);

        if (account is null)
        {
            return NotFound();
        }
        return account;
    }

    [HttpPost]
    public async Task<IActionResult> Create(AccountDTO account)
    {
        string validationResult = await ValidateAccount(account);

        if (!validationResult.Equals("Valid"))
        {
            return BadRequest(new { message = validationResult });
        }
        var newAccount = await accountservice.Create(account);

        return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, newAccount);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AccountDTO account)
    {
        if (id != account.Id)
        {
            return BadRequest(
                new
                {
                    message = $"El Id({id}) de la URL no coincide con el ID({account.Id}) de cuerpo de la solicitud:"
                }
            );
        }

        var accountToUpdate = await accountservice.GetById(id);

        if (accountToUpdate is not null)
        {
            string validationResult = await ValidateAccount(account);

            if (!validationResult.Equals("Valid"))
            {
                return BadRequest(new { message = validationResult });
            }
            await accountservice.Update(id, account);
            return NoContent();
        }
        else
        {
            return AccountNoFound(id);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var accountToDelete = await accountservice.GetById(id);

        if (accountToDelete is not null)
        {
            await accountservice.Delete(id);
            return Ok();
        }
        else
        {
            return AccountNoFound(id);
        }
    }

    public NotFoundObjectResult AccountNoFound(int id)
    {
        return NotFound(new { message = $"La cuenta con ID = {id} no existe." });
    }

    public async Task<string> ValidateAccount(AccountDTO account)
    {
        string result = "Valid";

        var accountType = await accountTypeService.GetById(account.AccountType);

        if (accountType is null)
        {
            result = $"El tipo de cuenta {account.AccountType} no existe";
        }

        var clientId = account.ClientId.GetValueOrDefault();

        var client = await clientService.GetById(clientId);

        if (client is null)
        {
            result = $"El cliente {clientId} no existe.";
        }

        return result;
    }
}
