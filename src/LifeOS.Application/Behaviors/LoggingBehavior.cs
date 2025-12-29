using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                // Yapılandırılmış loglama kullanarak istek başlatıldı
                _logger.LogInformation("{RequestType} isteği başlatılıyor", typeof(TRequest).Name);
                var result = await next();
                _logger.LogInformation("{RequestType} isteği tamamlandı", typeof(TRequest).Name);

                return result;
            }
            catch (Exception ex)
            {
                // Yapılandırılmış loglama ile hata kaydı
                _logger.LogError(ex, "{RequestType} isteği sırasında hata oluştu", typeof(TRequest).Name);
                throw;
            }
        }
    }
}
