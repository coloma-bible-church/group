namespace Group.WebApi.Controllers.Users
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Models;
    using Repositories.Users;

    [ApiController]
    [Route("api/v1/users")]
    [Authorize("SECRET")]
    public class UsersController : ControllerBase
    {
        readonly UsersRepository _usersRepository;
        readonly LinkGenerator _linkGenerator;

        public UsersController(
            UsersRepository usersRepository,
            LinkGenerator linkGenerator)
        {
            _usersRepository = usersRepository;
            _linkGenerator = linkGenerator;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var ids = await _usersRepository.GetIdsAsync(CancellationToken);
            return Ok(ids.Select(id => new ResourceModel(
                id,
                _linkGenerator.GetPathByAction("GetUser", "users", new { id })
            )));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            var userModel = await _usersRepository.ReadAsync(id, CancellationToken);
            if (userModel is null)
                return NotFound();
            return Ok(userModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutUser(string id, UserModel user)
        {
            await _usersRepository.UpsertAsync(id, user, CancellationToken);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            if (await _usersRepository.ReadAsync(id, CancellationToken) is null)
                return NotFound();
            await _usersRepository.DeleteAsync(id, CancellationToken);
            return Ok();
        }
    }
}