using Doodoo.Modules.Inventory.Contracts;
using Doodoo.Modules.Inventory.Services;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class InventoryController(
        InventoryService inventoryService,
        UserService userService) : ControllerBase
    {
        // Everything the frontend needs on startup, in one response.
        [HttpGet]
        public async Task<ActionResult<InventoryResponse>> Get()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var response = await inventoryService.GetInventoryAsync(userId);

            // Cheap validation / vangnet for the PWA cache: ETag on the version.
            var etag = $"\"{response.Version}\"";
            if (Request.Headers.IfNoneMatch == etag)
                return StatusCode(StatusCodes.Status304NotModified);

            Response.Headers.ETag = etag;
            return Ok(response);
        }

        [HttpPost("{entryId:int}/equip")]
        public async Task<ActionResult<InventoryResponse>> Equip(int entryId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await inventoryService.EquipAsync(userId, entryId);
            return MapResult(result);
        }

        [HttpPost("{entryId:int}/unequip")]
        public async Task<ActionResult<InventoryResponse>> Unequip(int entryId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await inventoryService.UnequipAsync(userId, entryId);
            return MapResult(result);
        }

        [HttpPost("{entryId:int}/use")]
        public async Task<ActionResult<UseItemResponse>> Use(int entryId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await inventoryService.UseAsync(userId, entryId);

            return result.Code switch
            {
                InventoryOpCode.Success => Ok(result.Value),
                InventoryOpCode.EntryNotFound => NotFound("Inventory entry not found."),
                InventoryOpCode.NotAConsumable => BadRequest("This item is not a consumable."),
                InventoryOpCode.OutOfStock => BadRequest("This item is out of stock."),
                InventoryOpCode.CurrencyGrantFailed => StatusCode(StatusCodes.Status502BadGateway, "Currency grant failed."),
                _ => BadRequest(result.Code.ToString())
            };
        }

        private ActionResult<InventoryResponse> MapResult(InventoryOperationResult<InventoryResponse> result) =>
            result.Code switch
            {
                InventoryOpCode.Success => Ok(result.Value),
                InventoryOpCode.EntryNotFound => NotFound("Inventory entry not found."),
                InventoryOpCode.NotACustomization => BadRequest("This item is not a customization."),
                _ => BadRequest(result.Code.ToString())
            };
    }
}
