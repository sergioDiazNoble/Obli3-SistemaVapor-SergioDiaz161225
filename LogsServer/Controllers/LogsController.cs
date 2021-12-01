using Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogsServer.Controllers
{
    [Route("logs")]
    [ApiController]
    public class LogsController : ControllerBase
    {

        [HttpGet]
        public async Task<List<object>> GetAll()
        {
            var response = await LogsManagment.GetAll();

            if (response != null)
            {
                return response;
            }

            return null;

        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<object>> GetLogByIdAsync(int id)
        {
            var responseLogsUser = await LogsManagment.GetLogByIdUser(id);
            var responseLogsGame = await LogsManagment.GetLogByIdGame(id);

            if (responseLogsUser != null)
                return Ok(responseLogsUser);
            
            if (responseLogsGame != null)
                return Ok(responseLogsGame);

            return NotFound();
        }





    }
}
