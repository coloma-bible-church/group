namespace Group.WebApi.Controllers.Contacts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Models;
    using Repositories.Users;

    [ApiController]
    [Route("api/v1/contacts")]
    [Authorize("SECRET")]
    public class ContactsController : ControllerBase
    {
        readonly UsersRepository _usersRepository;
        readonly LinkGenerator _linkGenerator;

        public ContactsController(
            UsersRepository usersRepository,
            LinkGenerator linkGenerator)
        {
            _usersRepository = usersRepository;
            _linkGenerator = linkGenerator;
        }

        CancellationToken CancellationToken => HttpContext.RequestAborted;

        [HttpGet("{kind}/{value}")]
        public async Task<ActionResult> Get(string kind, string value)
        {
            var id = await _usersRepository.GetIdFromContactAsync(
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
                _linkGenerator.GetPathByAction("GetUser", "users", new { id })
            ));
        }
    }
}