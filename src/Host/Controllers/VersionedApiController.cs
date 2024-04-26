using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
public class VersionedApiController : BaseApiController
{
}
