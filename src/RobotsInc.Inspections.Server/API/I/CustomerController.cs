using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using RobotsInc.Inspections.BusinessLogic;
using RobotsInc.Inspections.Server.Mappers;
using RobotsInc.Inspections.Server.Security;

using Swashbuckle.AspNetCore.Annotations;

using Customer = RobotsInc.Inspections.Models.Customer;

namespace RobotsInc.Inspections.Server.API.I;

[ApiV1]
[Route(
    Inspections.API.I.Routes.ApiVersion
    + Inspections.API.I.Routes.Customers)]
public class CustomerController
    : InspectionsController
{
    public const string IdentifierId = "customerId";
    public const string RouteId = "{" + IdentifierId + ":long:min(1)}";

    public CustomerController(
        ILogger<CustomerController> logger,
        ICustomerManager customerManager,
        IMapper<Customer, Inspections.API.I.Customer> customerMapper)
        : base(logger)
    {
        CustomerManager = customerManager;
        CustomerMapper = customerMapper;
    }

    public ICustomerManager CustomerManager { get; }
    public IMapper<Customer, Inspections.API.I.Customer> CustomerMapper { get; }

    /// <summary>
    ///     Create a new <see cref="Inspections.API.I.Customer" /> with the given properties.
    /// </summary>
    /// <param name="customer">the customer to create</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The created <see cref="Inspections.API.I.Customer" />.
    /// </returns>
    /// <response code="201">
    ///     The given <paramref name="customer" /> is created successfully.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="customer" /> does not satisfy the validation criteria and cannot be created.
    /// </response>
    // [ProducesResponseType(typeof(Inspections.API.I.Customer), StatusCodes.Status201Created, ApplicationJson)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, ApplicationProblemJson)]
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, null, typeof(Inspections.API.I.Customer), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateCustomer(Inspections.API.I.Customer customer, CancellationToken cancellationToken)
    {
        if (customer.Id != null)
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        Customer model = await CustomerMapper.MapAsync(customer, cancellationToken);
        await CustomerManager.SaveAsync(model, cancellationToken);
        Inspections.API.I.Customer dto = await CustomerMapper.MapAsync(model, cancellationToken);

        string? route =
            Url.Link(
                Routes.Customer_GetById,
                new { customerId = dto.Id });
        return Created(route!, dto);
    }

    /// <summary>
    ///     Retrieve the <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/>.
    /// </summary>
    /// <param name="customerId">the unique id of a customer</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/>.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> was found and
    ///     is returned.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> does not exist.
    /// </response>
    [HttpGet(RouteId, Name = Routes.Customer_GetById)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Customer), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.CONSULT_INSPECTIONS)]
    public async Task<IActionResult> RetrieveCustomer(long customerId, CancellationToken cancellationToken)
    {
        Customer? model = await CustomerManager.GetByIdAsync(customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        Inspections.API.I.Customer dto = await CustomerMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Update the <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/>.
    /// </summary>
    /// <param name="customerId">the unique id of a customer</param>
    /// <param name="customer">the updated properties for the customer</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> after the update.
    /// </returns>
    /// <response code="200">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> was found, was
    ///     updated and the updated <see cref="Inspections.API.I.Customer"/> is returned.
    /// </response>
    /// <response code="400">
    ///     The given <paramref name="customer"/> properties do not pass the validation requirements.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> does not exist.
    /// </response>
    [HttpPut(RouteId)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(Inspections.API.I.Customer), ApplicationJson)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(HttpValidationProblemDetails), ApplicationProblemJson)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateCustomer(long customerId, Inspections.API.I.Customer customer, CancellationToken cancellationToken)
    {
        Customer? model = await CustomerManager.GetByIdAsync(customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        if ((customer.Id != null) && (customer.Id != model.Id))
        {
            ProblemDetails problemDetails =
                new HttpValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        { "Id", new[] { "must not be given" } }
                    });
            return BadRequest(problemDetails);
        }

        await CustomerMapper.MapAsync(customer, model, cancellationToken);
        await CustomerManager.SaveAsync(model, cancellationToken);

        Inspections.API.I.Customer dto = await CustomerMapper.MapAsync(model, cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    ///     Delete the <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/>.
    /// </summary>
    /// <param name="customerId">the unique id of a customer</param>
    /// <param name="cancellationToken">pass-through cancellation token</param>
    /// <returns>
    ///     Does not return a result.
    /// </returns>
    /// <response code="204">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> was found and was
    ///     removed.
    /// </response>
    /// <response code="404">
    ///     The <see cref="Inspections.API.I.Customer"/> with the given <paramref name="customerId"/> does not exist.
    /// </response>
    [HttpDelete(RouteId)]
    [SwaggerResponse(StatusCodes.Status204NoContent)]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(ProblemDetails), ApplicationProblemJson)]
    [InspectionsAuthorize(Policy.EDIT_INSPECTIONS)]
    public async Task<IActionResult> DeleteCustomer(long customerId, CancellationToken cancellationToken)
    {
        Customer? model = await CustomerManager.GetByIdAsync(customerId, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        await CustomerManager.DeleteAsync(model, cancellationToken);
        return NoContent();
    }
}
