using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

[Route("api/[controller]")]
[ApiVersionNeutral]
public class VersionNeutralApiController : BaseApiController
{
}
