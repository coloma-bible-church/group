namespace Group.WebApi.Controllers.Contacts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Models;
    using Repositories.Users;

    [ApiController]
    [Route("api/v1/contacts")]
    public class ContactsController : ControllerBase
    {
        readonly UserRepository _userRepository;
        readonly LinkGenerator _linkGenerator;

        public ContactsController(
            UserRepository userRepository,
            LinkGenerator linkGenerator)
        {
            _userRepository = userRepository;
            _linkGenerator = linkGenerator;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        [HttpGet("{kind}/{value}")]
        public async Task<ActionResult> Get(string kind, string value)
        {
            var id = await _userRepository.GetIdFromContactAsync(
                new ContactModel
                {
                    Kind = kind,
                    Value = value
                },
                CancellationToken
            );
            if (id is null)
                return NotFound();
            return Ok(new ResourceModel(
                id,
                _linkGenerator.GetPathByAction("GetUser", "user", new { id })
            ));
        }
    }
}