using EdgeDB;
using HW12.Shared;
using Microsoft.AspNetCore.Mvc;

namespace HW12.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ContactController : Controller
{
    private readonly EdgeDBClient _client;

    public ContactController(EdgeDBClient client)
    {
        _client = client;
    }

    [HttpGet("GetAllContacts")]
    public async Task<IActionResult> GetAllContacts()
    {
        var query = $@"SELECT Contact {{ id, username, contactRole, firstName, lastName, email, description, birthDate, status, title}}";
        var contacts = await _client.QueryAsync<Contact>(query);

        if (contacts != null)
        {
            return Ok(contacts.ToList());
        }

        return BadRequest();
    }

    [HttpPost("AddContact")]
    public async Task<IActionResult> AddContact(Contact newContact)
    {
        if (string.IsNullOrEmpty(newContact.BirthDate) || (string.IsNullOrEmpty(newContact.FirstName)) || string.IsNullOrEmpty(newContact.Title)
            || (string.IsNullOrEmpty(newContact.LastName)) || (string.IsNullOrEmpty(newContact.Email)))
        {
            return BadRequest();
        }

        var query = "INSERT Contact { username := <str>$username, password := <str>$password, contactRole := <Role>$contactRole, firstName := <str>$firstName, lastName := <str>$lastName, email := <str>$email, description := <str>$description, title := <str>$title, status := <bool>$status, birthDate := <str>$birthDate } ";

        await _client.ExecuteAsync(query, new Dictionary<string, object?>
        {
            {"username",newContact.Username},
            {"password",newContact.Password},
            {"contactRole", newContact.ContactRole},
            {"firstName", newContact.FirstName},
            {"lastName", newContact.LastName},
            {"email", newContact.Email},
            {"title", newContact.Title},
            {"description", newContact.Description},
            {"birthDate", newContact.BirthDate},
            {"status", newContact.Status}
        });

        return Ok();
    }

    [HttpPost("EditContact")]
    public async Task<IActionResult> EditContact(Contact contactToUpdate)
    {
        var query = "UPDATE Contact" +
           " FILTER .username = <str>$username" +
           " SET { username := <str>$username, password := <str>$password, contactRole := <Role>$contactRole, firstName := <str>$firstName, lastName := <str>$lastName, email := <str>$email, description := <str>$description, title := <str>$title, status := <bool>$status, birthDate := <str>$birthDate }";

        await _client.ExecuteAsync(query, new Dictionary<string, object?>
        {
            {"username", contactToUpdate.Username},
            {"password", contactToUpdate.Password},
            {"contactRole", contactToUpdate.ContactRole},
            {"firstName", contactToUpdate.FirstName},
            {"lastName", contactToUpdate.LastName},
            {"email", contactToUpdate.Email},
            {"title", contactToUpdate.Title},
            {"description", contactToUpdate.Description},
            {"birthDate", contactToUpdate.BirthDate},
            {"status", contactToUpdate.Status}
        });

        return Ok();
    }

    [HttpGet("GetContact")]
    public async Task<IActionResult> GetContact(string username)
    {
        var contacts = await _client.QueryAsync<Contact>("SELECT Contact{username, description, email, firstName, lastName, status, title, birthDate, contactRole, password} FILTER .username = <str>$username",
           new Dictionary<string, object?> {
                    { "username", username }
           });

        return Ok(contacts.FirstOrDefault());
    }

    [HttpPost("DeleteContact")]
    public async Task<IActionResult> DeleteContact(string username)
    {
        var query = "DELETE Contact FILTER Contact .username = <str>$username";

        await _client.ExecuteAsync(query, new Dictionary<string, object?>
        {
            {"username", username }
        });

        return Ok();
    }

    [HttpGet("SearchForContact")]
    public async Task<IActionResult> SearchForContact(string searchWord)
    {
        var query = "SELECT Contact {*} FILTER .first_name ILIKE '%' ++ <str>$first_name ++ '%' OR .last_name ILIKE '%' ++ <str>$last_name ++ '%' OR .email ILIKE '%' ++ <str>$email ++ '%'";
        var contacts = await _client.QueryAsync<Contact>(query, new Dictionary<string, object?>
            {
                { "first_name", searchWord},
                { "last_name", searchWord },
                { "email", searchWord }
            });

        return Ok(contacts.ToList());
    }
}


