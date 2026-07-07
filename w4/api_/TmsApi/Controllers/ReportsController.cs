using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{

}