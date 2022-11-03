using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using RobotsInc.Inspections.Models;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Server.Mappers;

public class CustomerMapper : IMapper<Customer, Inspections.API.I.Customer>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerMapper(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    /// <inheritdoc />
    public async Task<Inspections.API.I.Customer> MapAsync(Customer model, CancellationToken cancellationToken)
    {
        Inspections.API.I.Customer dto = new();
        await MapAsync(model, dto, cancellationToken);
        return dto;
    }

    /// <inheritdoc />
    public Task MapAsync(Customer model, Inspections.API.I.Customer dto, CancellationToken cancellationToken)
    {
        dto.Id = model.Id;
        dto.Name = model.Name;
        dto.Description = model.Description;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Customer> MapAsync(Inspections.API.I.Customer dto, CancellationToken cancellationToken)
    {
        Customer model;

        if (dto.Id != null)
        {
            Customer? customer = await _customerRepository.GetByIdAsync(dto.Id.Value, cancellationToken);
            model = customer ?? throw new BadHttpRequestException("Bad id");
        }
        else
        {
            model = new();
        }

        await MapAsync(dto, model, cancellationToken);
        return model;
    }

    /// <inheritdoc />
    public Task MapAsync(Inspections.API.I.Customer dto, Customer model, CancellationToken cancellationToken)
    {
        model.Name = dto.Name;
        model.Description = dto.Description;
        return Task.CompletedTask;
    }
}
