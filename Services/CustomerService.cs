using AutoMapper;
using JekirdekCase.Dtos;
using JekirdekCase.Interfaces;
using JekirdekCase.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JekirdekCase.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper; 
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, IMapper mapper, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            if (createCustomerDto == null)
                throw new ArgumentNullException(nameof(createCustomerDto));

            var existingCustomerByEmail = await _customerRepository.GetByEmailAsync(createCustomerDto.Email);
            if (existingCustomerByEmail != null)
            {
                _logger.LogWarning("Attempted to create customer with existing email: {Email}", createCustomerDto.Email);
                throw new InvalidOperationException($"A customer with email '{createCustomerDto.Email}' already exists.");
            }

            // DTO'dan Entity'ye dönüşüm
            var customerEntity = _mapper.Map<Customer>(createCustomerDto);

            if (customerEntity.RegistrationDate.Kind != DateTimeKind.Utc)
            {
                customerEntity.RegistrationDate = DateTime.SpecifyKind(customerEntity.RegistrationDate, DateTimeKind.Utc);
            }

            await _customerRepository.AddAsync(customerEntity);
            await _customerRepository.SaveChangesAsync();
            _logger.LogInformation("Customer created with ID: {CustomerId} and Email: {Email}", customerEntity.Id, customerEntity.Email);

            // Oluşturulan Entity'den DTO'ya dönüşüm
            return _mapper.Map<CustomerDto>(customerEntity);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customerToDelete = await _customerRepository.GetByIdAsync(id);
            if (customerToDelete == null)
            {
                _logger.LogWarning("Attempted to delete non-existing customer with ID: {CustomerId}", id);
                return false;
            }

            _customerRepository.Remove(customerToDelete);
            var result = await _customerRepository.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation("Customer deleted with ID: {CustomerId}", id);
                return true;
            }
            _logger.LogError("Failed to delete customer with ID: {CustomerId}", id);
            return false;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            _logger.LogInformation("Fetching all customers.");
            var customers = await _customerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            _logger.LogInformation("Fetching customer by email: {Email}", email);
            var customer = await _customerRepository.GetByEmailAsync(email);
            return customer == null ? null : _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            _logger.LogInformation("Fetching customer by ID: {CustomerId}", id);
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer == null ? null : _mapper.Map<CustomerDto>(customer);
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomersFilteredAsync(
            string? name, string? email, DateTime? registrationDateFrom,
            DateTime? registrationDateTo, string? region)
        {
            _logger.LogInformation("Fetching filtered customers with criteria - Name: {Name}, Email: {Email}, DateFrom: {DateFrom}, DateTo: {DateTo}, Region: {Region}",
                                   name, email, registrationDateFrom, registrationDateTo, region);
            var customers = await _customerRepository.GetCustomersFilteredAsync(name, email, registrationDateFrom, registrationDateTo, region);
            // Entity listesinden DTO listesine dönüşüm
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<bool> UpdateCustomerAsync(UpdateCustomerDto updateCustomerDto)
        {
            if (updateCustomerDto == null)
                throw new ArgumentNullException(nameof(updateCustomerDto));

            var existingCustomer = await _customerRepository.GetByIdAsync(updateCustomerDto.Id);
            if (existingCustomer == null)
            {
                _logger.LogWarning("Attempted to update non-existing customer with ID: {CustomerId}", updateCustomerDto.Id);
                return false;
            }

            // email uniq kontrolu
            if (existingCustomer.Email.ToLower() != updateCustomerDto.Email.ToLower())
            {
                var customerWithNewEmail = await _customerRepository.GetByEmailAsync(updateCustomerDto.Email);
                if (customerWithNewEmail != null && customerWithNewEmail.Id != updateCustomerDto.Id)
                {
                    _logger.LogWarning("Attempted to update customer ID {CustomerId} with an email ({Email}) that already exists for another customer ID {ExistingCustomerId}",
                        updateCustomerDto.Id, updateCustomerDto.Email, customerWithNewEmail.Id);
                    throw new InvalidOperationException($"The email '{updateCustomerDto.Email}' is already in use by another customer.");
                }
            }

            // DTO'dan var olan Entity'ye değerleri map'le
            _mapper.Map(updateCustomerDto, existingCustomer);

            if (existingCustomer.RegistrationDate.Kind != DateTimeKind.Utc)
            {
                existingCustomer.RegistrationDate = DateTime.SpecifyKind(existingCustomer.RegistrationDate, DateTimeKind.Utc);
            }

            _customerRepository.Update(existingCustomer);
            var result = await _customerRepository.SaveChangesAsync();

            if (result > 0)
            {
                _logger.LogInformation("Customer updated with ID: {CustomerId}", updateCustomerDto.Id);
                return true;
            }
            _logger.LogError("Failed to update customer with ID: {CustomerId}", updateCustomerDto.Id);
            return false;
        }
    }
}
