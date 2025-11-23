using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Application.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace FCG_Payments.Api.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentContoller(IPaymentService service) : ControllerBase
    {
        /// <summary>
        /// Efetua um pagamento.
        /// </summary>
        /// <param name="paymentId">Código do pagamento</param>
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IResult> PayOrderAsync([FromBody] Guid paymentId, CancellationToken cancellationToken = default)
        {
            var result = await service.PayAsync(paymentId, cancellationToken);

            if(result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "404" => TypedResults.NotFound(new Error("404", result.Error.Message)),
                    "402" => TypedResults.StatusCode(StatusCodes.Status402PaymentRequired),
                    _ => TypedResults.BadRequest(new Error("400", result.Error.Message))
                };
            }

            return TypedResults.Ok(result.Value);
        }
    }
}
