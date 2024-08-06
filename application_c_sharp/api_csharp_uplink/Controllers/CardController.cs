using Microsoft.AspNetCore.Mvc;
using api_csharp_uplink.Dto;
using api_csharp_uplink.DirException;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Interface;

namespace api_csharp_uplink.Controllers;

/// <summary>
/// Controller to manage Card.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CardController(ICardFinder cardFinder, ICardRegistration cardRegistration) : ControllerBase
{
    /// <summary>
    /// Register a new card depending on lineNumber and his DevEUI code
    /// </summary>
    /// <param name="cardDto">The schema card DTO with lineNumber and devEuiCard</param>
    /// <returns>A Card created if success or a conflict status if the card is already created</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Card), 201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AddCard([FromBody] CardDto cardDto)
    {
        try
        {
            Card cardAdded = await cardRegistration.CreateCard(cardDto.LineBus, cardDto.DevEuiCard);
            return Created($"api/Card/devEuiCard/{cardDto.DevEuiCard}", cardAdded);
        }
        catch(Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
    
    /// <summary>
    /// Modify a card depending on lineNumber and his devEUI code
    /// </summary>
    /// <param name="cardDto">The schema card DTO with lineNumber and devEuiCard</param>
    /// <returns>A Card created if success is the card is already created</returns>
    [HttpPut]
    [ProducesResponseType(typeof(Card), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ModifyCard([FromBody] CardDto cardDto)
    {
        try
        {
            Card cardModify = await cardRegistration.ModifyCard(cardDto.LineBus, cardDto.DevEuiCard);
            return Ok(cardModify);
        }
        catch(Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }

    /// <summary>
    /// Get a card depending on devEUI number
    /// </summary>
    /// <param name="devEuiCard">The devEUI of the card</param>
    /// <returns>A response with card or a NotFound response</returns>
    [HttpGet("devEuiCard/{devEuiCard}")]
    [ProducesResponseType(typeof(Card), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCardByDevEui(string devEuiCard)
    {
        try
        {
            Card cardFind = await cardFinder.GetCardByDevEuiCard(devEuiCard);
            return Ok(cardFind);
        }
        catch(Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }

    /// <summary>
    /// Get all cards
    /// </summary>
    /// <returns>A list of card</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Card>), 200)]
    public async Task<IActionResult> GetCards()
    {
        try
        {
            List<Card> cards = await cardFinder.GetCards();
            return Ok(cards);
        }
        catch(Exception e)
        {
            return ErrorManager.HandleError(e);
        }
    }
}
