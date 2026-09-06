using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{

}