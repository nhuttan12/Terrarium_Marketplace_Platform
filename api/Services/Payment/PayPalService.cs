using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using api.Extensions;

namespace api.Services.Payment
{

    public class PayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayPalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Lấy BaseUrl từ cấu hình, mặc định dùng Sandbox nếu không tìm thấy
            var baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        // 1. Hàm lấy Access Token từ PayPal
        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = EnvironmentVariables.PayPalClientId;
            var secret = EnvironmentVariables.PayPalClientSecret;
            
            var authenticationString = $"{clientId}:{secret}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return tokenResult.GetProperty("access_token").GetString()!;
        }

        // 2. Hàm tạo đơn hàng trên PayPal (Truyền InvoiceId vào để đối soát)
        public async Task<string> CreateOrderAsync(string invoiceId, decimal amount)
        {
            var accessToken = await GetAccessTokenAsync();

            var orderRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        custom_id = invoiceId, // Nhúng mã InvoiceId của hệ thống bạn vào đây
                        amount = new
                        {
                            currency_code = "USD",
                            value = amount.ToString("0.00") // Bắt buộc format 2 số thập phân
                        }
                    }
                }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders")
            {
                Content = new StringContent(JsonSerializer.Serialize(orderRequest), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi tạo PayPal Order: {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return result.GetProperty("id").GetString()!; // Trả về PayPal Order ID
        }

        // 3. Hàm chốt giao dịch (Thu tiền)
        public async Task<string> CaptureOrderAsync(string paypalOrderId)
        {
            var accessToken = await GetAccessTokenAsync();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{paypalOrderId}/capture")
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json") // Bắt buộc phải có body rỗng
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi khi chốt PayPal Capture: {error}");
            }

            // Trả về toàn bộ chuỗi JSON để PaymentService bóc tách dữ liệu và lưu vào database
            return await response.Content.ReadAsStringAsync();
        }
    }
}
