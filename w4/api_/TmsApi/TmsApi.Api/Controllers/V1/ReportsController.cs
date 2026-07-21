using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{

}