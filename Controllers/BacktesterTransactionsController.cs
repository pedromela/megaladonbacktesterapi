using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BacktesterLib.Models;
using BotLib.Models;
using LoginLib.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using static BrokerLib.BrokerLib;

namespace BacktesterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BacktesterTransactionsController : ControllerBase
    {
        private readonly BacktesterDBContext _backtesterContext;
        private readonly BotDBContext _botContext;
        private UserManager<ApplicationUser> _userManager;

        public BacktesterTransactionsController(
            BacktesterDBContext backtesterContext,
            BotDBContext botContext,
            UserManager<ApplicationUser> userManager
        )
        {
            _botContext = botContext;
            _backtesterContext = backtesterContext;
            _userManager = userManager;
        }

        private async Task<ActionResult> CheckBotParametersRights(string botId)
        {
            try
            {
                string userId = User.Claims.First(c => c.Type == "UserID").Value;
                ApplicationUser user = await _userManager.FindByIdAsync(userId);

                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound();
                }

                if (!user.UserName.Equals("admin")) //if admin, dont need any more checks
                {
                    List<UserBotRelation> userBotRelations = await _botContext.UserBotRelations.Where(m => m.UserId == userId).ToListAsync();
                    bool success = false;
                    foreach (var relation in userBotRelations)
                    {
                        if (relation.BotId == botId)
                        {
                            success = true;
                            break;
                        }
                    }

                    if (!success)
                    {
                        return Unauthorized();
                    }
                    return Ok();
                }
                return Ok();
            }
            catch (Exception e)
            {
                BotLib.BotLib.DebugMessage(e);
            }
            return NotFound();
        }

        // GET: api/BacktesterTransactions/history/5/0
        [HttpGet("historybycount/{count}/{botId}/{timestamp}")]
        [Authorize/*(Roles ="admin")*/]
        public async Task<ActionResult<IEnumerable<BacktesterTransaction>>> GetTradeHistoryByBotId(int count, string botId, DateTime timestamp)
        {
            List<BacktesterTransaction> resultTransactions = new List<BacktesterTransaction>();
            try
            {
                var result = await CheckBotParametersRights(botId);
                if (result.GetType() != typeof(OkResult))
                {
                    return result;
                }

                List<BacktesterTransaction> transactions = null;

                if (timestamp == DateTime.MinValue)
                {
                    transactions = await _backtesterContext.BacktesterTransactions.Where(m => m.BotId == botId && (m.Type == TransactionType.buyclose || m.Type == TransactionType.sellclose)).OrderByDescending(m => m.id).Take(count).ToListAsync();
                }
                else
                {
                    transactions = await _backtesterContext.BacktesterTransactions.Where(m => m.BotId == botId && (m.Type == TransactionType.buyclose || m.Type == TransactionType.sellclose) && m.Timestamp < timestamp).OrderByDescending(m => m.Timestamp).Take(count).ToListAsync();
                }
                var transactionsAux = transactions;
                foreach (var t in transactionsAux)
                {
                    var transaction = _backtesterContext.BacktesterTransactions.Find(t.BuyId);
                    if (transaction != null && (transaction.Type == TransactionType.buydone || transaction.Type == TransactionType.selldone))
                    {
                        resultTransactions.Add(transaction);
                        resultTransactions.Add(t);
                    }
                    else
                    {
                        BotLib.BotLib.DebugMessage("ERROR: GetTradeHistoryByBotId(int count) SellTransaction without BuyTransactionId or invalid type!!");
                        //transactions.Remove(t);
                    }
                }
                return resultTransactions;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return resultTransactions;
        }

        // GET: api/BacktesterTransactions/active/5
        [HttpGet("activebycount/{count}/{botId}/{timestamp}")]
        [Authorize/*(Roles ="admin")*/]
        public async Task<ActionResult<IEnumerable<BacktesterTransaction>>> GetActiveTransactionsByBotId(int count, string botId, DateTime timestamp)
        {
            try
            {
                var result = await CheckBotParametersRights(botId);
                if (result.GetType() != typeof(OkResult))
                {
                    return result;
                }

                List<BacktesterTransaction> transactions = null;

                if (timestamp == DateTime.MinValue)
                {
                    transactions = await _backtesterContext.BacktesterTransactions.Where(m => m.BotId == botId &&
                                    (m.Type == TransactionType.buy || m.Type == TransactionType.sell))
                                    .OrderByDescending(m => m.Timestamp)
                                    .Take(count)
                                    .ToListAsync();
                }
                else
                {
                    transactions = await _backtesterContext.BacktesterTransactions.Where(m => m.BotId == botId &&
                                    (m.Type == TransactionType.buy || m.Type == TransactionType.sell) &&
                                    m.Timestamp < timestamp)
                                    .OrderByDescending(m => m.Timestamp)
                                    .Take(count)
                                    .ToListAsync();
                }

                return transactions;
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }

        // GET: api/BacktesterTransactions/history/5/0
        [HttpGet("history")]
        [Authorize/*(Roles ="admin")*/]
        public async Task<ActionResult<IEnumerable<BacktesterTransaction>>> GetTradeHistoryByBotIdFromTo(string botId, DateTime from, DateTime to)
        {
            List<BacktesterTransaction> transactions = new List<BacktesterTransaction>();
            try
            {
                var result = await CheckBotParametersRights(botId);
                if (result.GetType() != typeof(OkResult))
                {
                    return result;
                }

                transactions = await _backtesterContext.BacktesterTransactions.Where(m =>
                    m.BotId == botId &&
                    (m.Type == TransactionType.buyclose ||
                    m.Type == TransactionType.sellclose ||
                    m.Type == TransactionType.buydone ||
                    m.Type == TransactionType.selldone) &&
                    m.Timestamp >= from &&
                    m.Timestamp < to)
                    .ToListAsync();

                return transactions;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return transactions;
        }

        // GET: api/BacktesterTransactions/active/5
        [HttpGet("active")]
        [Authorize/*(Roles ="admin")*/]
        public async Task<ActionResult<IEnumerable<BacktesterTransaction>>> GetActiveTransactionsByBotIdFromTo(string botId, DateTime from, DateTime to)
        {
            try
            {
                var result = await CheckBotParametersRights(botId);
                if (result.GetType() != typeof(OkResult))
                {
                    return result;
                }

                List<BacktesterTransaction> transactions = await _backtesterContext.BacktesterTransactions.Where(m => m.BotId == botId &&
                                    (m.Type == TransactionType.buy || m.Type == TransactionType.sell) &&
                                    m.Timestamp >= from &&
                                    m.Timestamp < to)
                                    .ToListAsync();


                return transactions;
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
