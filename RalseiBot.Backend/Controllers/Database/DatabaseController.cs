using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Authorize]
[Route("[controller]")]
public class DatabaseController([FromKeyedServices("ServerDB")] MySqlDataSource serverDbSource) : ControllerBase
{
}