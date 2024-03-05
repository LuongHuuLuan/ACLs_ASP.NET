using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACLAuthorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ACLs")]
    public class ResourcesController: ControllerBase
    {
        private readonly EndpointDataSource _endpointDataSource;

        public ResourcesController(EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource ?? throw new ArgumentNullException(nameof(endpointDataSource));
        }

        [HttpGet("Endpoints")]
        public IActionResult GetEndpoints()
        {
            var endpointInfos = new List<string>();

            // Lặp qua danh sách tất cả các endpoint trong ứng dụng
            foreach (var endpoint in _endpointDataSource.Endpoints)
            {
                // Kiểm tra xem endpoint có phải là RouteEndpoint không
                if (endpoint is RouteEndpoint routeEndpoint)
                {
                    // Lấy pattern của route
                    var pattern = routeEndpoint.RoutePattern.RawText;

                    // Lấy thông tin về phương thức HTTP của endpoint (nếu có)
                    var httpMethodMetadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();

                    // Xây dựng thông tin về endpoint và phương thức HTTP
                    string endpointInfo = $"{endpoint.DisplayName} - ";

                    if (httpMethodMetadata != null)
                    {
                        endpointInfo += $"Method: {string.Join(", ", httpMethodMetadata.HttpMethods)} - ";
                    }

                    endpointInfo += $"Route: {pattern}";

                    // Thêm thông tin về endpoint vào danh sách
                    endpointInfos.Add(endpointInfo);
                }
            }

            // Trả về danh sách các endpoint
            return Ok(endpointInfos);
        }
    }
}
