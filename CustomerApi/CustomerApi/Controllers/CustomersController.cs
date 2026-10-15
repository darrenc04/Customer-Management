using CustomerApi.Data;
using CustomerApi.Dtos;
using CustomerApi.Exceptions;
using CustomerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CustomerApi.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]

public class CustomersController : ControllerBase
{
    private const int MaxSearchLength = 100;

    private readonly CustomerRepository _customerRepository;

    public CustomersController(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    //GET api/customers                     -> all customers
    //GET api/customers?search=searchText   -> search customers by name or email
    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll([FromQuery] string? searchText)
    {
        string text = (searchText ?? string.Empty).Trim();
        if (text.Length > MaxSearchLength)
            text = text[..MaxSearchLength];

        List<Customer> customers = text.Length == 0
            ? await _customerRepository.GetAllAsync()
            : await _customerRepository.SearchAsync(text);

        return Ok(customers);
    }

    //GET api/customers/{id} -> get customer by id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        Customer? customer = await _customerRepository.GetByIdAsync(id);
        return customer is null ? NotFound() : Ok(customer);
    }

    //POST api/customers -> create new customer
    [HttpPost]
    public async Task<ActionResult<Customer>> Create(CustomerRequest request)
    {
        var customer = new Customer
        {
            Username = request.Username.Trim(),
            Age = request.age,
            Contact = request.Contact.Trim(),
            Email = request.Email.Trim(),
            CreatedBy = User.Identity?.Name!
        };

        try
        {
            int newId = await _customerRepository.InsertAsync(customer);
            Customer? created = await _customerRepository.GetByIdAsync(newId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, created);
        }
        catch (DuplicateUsernameException)
        {
            return Conflict(DuplicateUsernameProblem());
        }
    }

    //PUT api/customers/{id} -> update existing customer
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, CustomerRequest request)
    {
        var customer = new Customer
        {
            CustomerID = id,
            Username = request.Username.Trim(),
            Age = request.age,
            Contact = request.Contact.Trim(),
            Email = request.Email.Trim(),
        };

        try
        {
            await _customerRepository.UpdateAsync(customer);
            return NoContent();
        }
        catch (DuplicateUsernameException)
        {
            return Conflict(DuplicateUsernameProblem());
        }
        catch (CustomerNotFoundException)
        {
            return NotFound();
        }
    }

    //HTTP DELETE api/customers/{id} -> delete customer by id
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _customerRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (CustomerNotFoundException)
        {
            return NotFound();
        }
    }

    private static ProblemDetails DuplicateUsernameProblem() => new()
    {
        Title = "Duplicated Username",
        Status = StatusCodes.Status409Conflict,
        Detail = "The username is already taken."
    };
}

