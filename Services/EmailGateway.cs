using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace FixItNR.Api.Services
{
    /// <summary>Настройки интеграции с email-backend.</summary>
    public sealed class EmailApiOptions
    {
        /// <summary>Полный URL до эндпоинта отправки (например, https://.../api/notify/email).</summary>
        public string NotifyUrl { get; set; } = string.Empty;

        /// <summary>Таймаут HTTP-клиента, сек.</summary>
        public int TimeoutSeconds { get; set; } = 10;
    }

    /// <summary>Контракт запроса к email-backend.</summary>
    public sealed record EmailNotifyRequest(
        string EmailTo,
        string AuthorName,
        string Category,
        string Place,
        string Description,
        string Priority,
        string Subject
    );

    /// <summary>Результат отправки письма.</summary>
    public sealed record EmailSendResult(bool Sent, string Info);

    /// <summary>Шлюз отправки писем.</summary>
    public interface IEmailGateway
    {
        /// <summary>Отправить письмо через email-backend.</summary>
        Task<EmailSendResult> SendAsync(EmailNotifyRequest req, CancellationToken ct = default);
    }

    /// <inheritdoc />
    public sealed class EmailGateway : IEmailGateway
    {
        private readonly HttpClient _http;
        private readonly EmailApiOptions _opt;

        public EmailGateway(HttpClient http, IOptions<EmailApiOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<EmailSendResult> SendAsync(EmailNotifyRequest req, CancellationToken ct = default)
        {
            // Если BaseAddress не настроен — постим по абсолютному NotifyUrl
            var url = _http.BaseAddress is null ? _opt.NotifyUrl : string.Empty;

            using var resp = await _http.PostAsJsonAsync(url, req, ct);
            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<EmailSendResult>(cancellationToken: ct);
            return dto ?? new EmailSendResult(false, "empty response");
        }
    }
}
