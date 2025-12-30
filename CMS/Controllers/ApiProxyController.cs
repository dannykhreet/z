using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Filters;
using WebApp.Logic.Interfaces;

namespace WebApp.Controllers
{
    //REMOVED; WHEN FULLY VALIDATED THIS FILE CAN BE DELETED.

    ///// <summary>
    ///// ApiProxyController for use within the application. All calls to this controller will be submitted to the api.
    ///// </summary>
    //[ServiceFilter(typeof(ValidTokenFilter)), Authorize(Roles = "manager,shift_leader")]
    //public class ApiProxyController : Controller
    //{
    //    private readonly IApiConnector _connector;

    //    public ApiProxyController(IApiConnector connector)
    //    {
    //        _connector = connector;
    //    }

    //    [Route("api/get/{*apiroutes}")]
    //    [HttpGet]
    //    public async Task<IActionResult> GetApiData(string apiroutes)
    //    {
    //        //Pushing the API routes to the API and returning the data so it can be used within Javascript without displaying the security data.
    //        //GetCalls will auto-append (if available) the token headers for a call to the API.
    //        var data = await _connector.GetCall(url: apiroutes);
    //        return StatusCode((int)data.StatusCode, data.Message);
    //    }


    //    [Route("api/post/{*apiroutes}")]
    //    [HttpPost]
    //    public async Task<IActionResult> PostApiData([FromRoute] string apiroutes, [FromBody] object jsonBody)
    //    {
    //        //Pushing the API routes to the API and returning the data so it can be used within Javascript without displaying the security data.
    //        //GetCalls will auto-append (if available) the token headers for a call to the API.
    //        var data = await _connector.PostCall(url: apiroutes, body: jsonBody.ToString());
    //        return StatusCode((int)data.StatusCode, data.Message);
    //    }
    //}
}
