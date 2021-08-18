namespace Group.WebApi.Controllers.Users
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Models;
    using Repositories.Users;

    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        readonly UserRepository _userRepository;
        readonly LinkGenerator _linkGenerator;

        public UserController(
            UserRepository userRepository,
            LinkGenerator linkGenerator)
        {
            _userRepository = userRepository;
            _linkGenerator = linkGenerator;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var ids = await _userRepository.GetIdsAsync(CancellationToken);
            return Ok(ids.Select(id => new ResourceModel(
                id,
                _linkGenerator.GetPathByAction("GetUser", "user", new { id })
            )));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetUser(string id)
        {
            var userModel = await _userRepository.ReadAsync(id, CancellationToken);
            if (userModel is null)
                return NotFound();
            return Ok(userModel);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> UpdateUser(string id, UserModel user)
        {
            await _userRepository.UpdateAsync(id, user, CancellationToken);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> AddUser(UserModel user)
        {
            var id = await _userRepository.CreateAsync(user, CancellationToken);
            return Ok(new ResourceModel(
                id,
                _linkGenerator.GetPathByAction("GetUser", "user", new { id })
            ));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            if (await _userRepository.ReadAsync(id, CancellationToken) is null)
                return NotFound();
            await _userRepository.DeleteAsync(id, CancellationToken);
            return Ok();
        }
    }
}