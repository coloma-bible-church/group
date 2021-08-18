namespace Group.WebApi.Controllers.Contacts
{
    using System.Linq;
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

        [HttpGet]
        public async Task<ActionResult> GetKinds()
        {
            var kinds = await _userRepository.GetContactKindsAsync(CancellationToken);
            return Ok(kinds.Select(kind => new ResourceModel(
                kind,
                _linkGenerator.GetPathByAction("Get", "contacts", new { kind })
            )));
        }

        [HttpGet("{kind}")]
        public async Task<ActionResult> Get(string kind, [FromQuery] string? value = null)
        {
            if (value is null)
            {
                var contacts = await _userRepository.GetContactsByKind(kind, CancellationToken);
                return Ok(contacts.Select(contact => new ResourceModel(
                    contact,
                    _linkGenerator.GetPathByAction("Get", "contacts", new { kind, value = contact })
                )));
            }

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