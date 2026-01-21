using FCG.Shared.Contracts.Enums;
using FCG.Shared.Contracts.Results;
using FCG_Payments.Application.Payments.Requests;
using FCG_Payments.Application.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG_Payments.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class PaymentController(IPaymentService service) : ControllerBase
    {
        /// <summary>
        /// Solicita a criação de um pagamento.
        /// </summary>
        /// <param name="request">Dados para gerar o pagamento</param>       
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("create")]
        public async Task<IResult> CreatePaymentAsync([FromBody] PaymentRequest request, CancellationToken cancellationToken = default)
        {
            var correlationId = HttpContext.Items["CorrelationId"]!.ToString();
            var result = await service.CreatePaymentAsync(request, correlationId!, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "404" => TypedResults.NotFound(new Error("404", result.Error.Message)),
                    _ => TypedResults.BadRequest(new Error("400", result.Error.Message))
                };
            }
            return TypedResults.Accepted($"/payments/{result.Value.Id}", new { Payment = result.Value, CorrelationId = correlationId });
        }

        /// <summary>
        /// Solicita a efetivação de um pagamento.
        /// </summary>
        /// <param name="paymentId">Id do pagamento</param>
        /// <param name="paymentType">Forma de pagamento (Free = 0, CreditCard = 1, DebitCard = 2, PayPal = 3, Pix = 4) </param>
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("pay")]
        public async Task<IResult> PayOrderAsync([FromBody] Guid paymentId, EPaymentType paymentType, CancellationToken cancellationToken = default)
        {
            var correlationId = HttpContext.Items["CorrelationId"]!.ToString();
            var result = await service.PayAsync(paymentId, paymentType, correlationId!, cancellationToken);

            if (result.IsFailure)
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

        /// <summary>
        /// Busca um pagamento por Id.
        /// </summary>
        /// <param name="paymentId">Id do pagamento</param>
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{paymentId}")]
        public async Task<IResult> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var result = await service.GetPaymentByIdAsync(paymentId, cancellationToken);
            if (result is null)
                return TypedResults.NotFound(new Error("404", "Pagamento não encontrado."));

            return TypedResults.Ok(result.Value);
        }

        /// <summary>
        /// Busca todos os pagamentos aprovados
        /// </summary>       
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("approved")]
        public async Task<IResult> GetApprovedPaymentsAsync(CancellationToken cancellationToken = default)
        {
            var result = await service.GetApprovedPaymentsAsync(cancellationToken);
           
            return TypedResults.Ok(result.Value);
        }

        /// <summary>
        /// Busca todos os pagamentos que falharam
        /// </summary>       
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("failed")]
        public async Task<IResult> GetFailedPaymentsAsync(CancellationToken cancellationToken = default)
        {
            var result = await service.GetFailedPaymentsAsync(cancellationToken);

            return TypedResults.Ok(result.Value);
        }

        /// <summary>
        /// Busca todos os pagamentos pendentes
        /// </summary>       
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("pending")]
        public async Task<IResult> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            var result = await service.GetPendingPaymentsAsync(cancellationToken);

            return TypedResults.Ok(result.Value);
        }
    }
}
