using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BacktesterAPI.DataStorage;
using BacktesterAPI.HubConfig;
using BacktesterAPI.TimerFeatures;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using BacktesterAPI.Manager;

namespace BacktesterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BacktesterController : ControllerBase
    {
        private BacktesterManager _backtesterManager;

        public BacktesterController(BacktesterManager backtesterManager) 
        {
            _backtesterManager = backtesterManager;
        }

        // GET: api/Backtester/
        [HttpGet]
        //[Authorize]
        public IActionResult BacktestBot(string botId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(botId))
            {
                return BadRequest(new { Message = "BotId is undefined." });
            }
            if (fromDate >= toDate)
            {
                return BadRequest(new { Message = "start date is is greater or equal than end date." });
            }
            _backtesterManager.StartBacktestBot(botId, fromDate, toDate);
            return Ok(new { Message = "Backtest Request Started" }); 
        }
    }
}