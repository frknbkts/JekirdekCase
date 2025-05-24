using JekirdekCase.Dtos;
using JekirdekCase.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JekirdekCase.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    // ESKİ: [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // GET: api/customers
        [HttpGet]
        // ESKİ: [Authorize(Roles = "Admin,User")]
        [Authorize(Roles = "Admin,User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] DateTime? registrationDateFrom,
            [FromQuery] DateTime? registrationDateTo,
            [FromQuery] string? region)
        {
            _logger.LogInformation("Attempting to get all customers with filters.");
            var customers = await _customerService.GetCustomersFilteredAsync(name, email, registrationDateFrom, registrationDateTo, region);
            return Ok(customers);
        }
        // GET: api/customers/5
        [HttpGet("{id}")]
        // ESKİ: [Authorize(Roles = "Admin,User")]
        [Authorize(Roles = "Admin,User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            _logger.LogInformation("Attempting to get customer with ID: {CustomerId}", id);
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                _logger.LogWarning("Customer with ID: {CustomerId} not found.", id);
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost]
        // ESKİ: [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to create a new customer with email: {Email}", createCustomerDto.Email);
            try
            {
                var createdCustomer = await _customerService.CreateCustomerAsync(createCustomerDto);
                if (createdCustomer == null)
                {
                    return BadRequest(new { message = "Customer creation failed. Email might already exist." });
                }
                return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (InvalidOperationException ex) // CustomerService'ten fırlatılan email unique hatası için
            {
                _logger.LogWarning(ex, "Error creating customer: {ErrorMessage}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating customer.");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        // ESKİ: [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateCustomerDto)
        {
            if (id != updateCustomerDto.Id)
            {
                return BadRequest(new { message = "ID mismatch between route and body." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to update customer with ID: {CustomerId}", id);
            try
            {
                var success = await _customerService.UpdateCustomerAsync(updateCustomerDto);
                if (!success)
                {
                    var existingCustomer = await _customerService.GetCustomerByIdAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound(new { message = $"Customer with ID {id} not found for update." });
                    }
                    return BadRequest(new { message = "Customer update failed for an unknown reason." });
                }
            }
            catch (InvalidOperationException ex) // CustomerService'ten fırlatılan email unique hatası için
            {
                _logger.LogWarning(ex, "Error updating customer: {ErrorMessage}", ex.Message);
                return Conflict(new { message = ex.Message }); // 409 Conflict
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating customer ID {CustomerId}.", id);
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }


            return NoContent();
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        // ESKİ: [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _logger.LogInformation("Attempting to delete customer with ID: {CustomerId}", id);
            var success = await _customerService.DeleteCustomerAsync(id);

            if (!success)
            {
                _logger.LogWarning("Customer with ID: {CustomerId} not found for deletion or delete failed.", id);
                return NotFound(new { message = $"Customer with ID {id} not found or delete operation failed." });
            }

            return NoContent();
        }
    }
}
