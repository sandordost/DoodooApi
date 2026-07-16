using Doodoo.Modules.Inventory.Contracts;
using Doodoo.Modules.Inventory.Entities;
using Doodoo.Modules.Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/admin/inventory")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminInventoryController(InventoryService inventoryService) : ControllerBase
    {
        [HttpGet("definitions")]
        public async Task<ActionResult<List<ItemDefinition>>> ListDefinitions()
        {
            return Ok(await inventoryService.ListDefinitionsAsync());
        }

        [HttpPost("definitions")]
        public async Task<ActionResult<ItemDefinition>> CreateDefinition(CreateItemDefinitionRequest request)
        {
            var result = await inventoryService.CreateDefinitionAsync(request);
            return result.Code switch
            {
                InventoryOpCode.Success => CreatedAtAction(nameof(ListDefinitions), new { }, result.Value),
                InventoryOpCode.DuplicateKey => Conflict($"A definition with key '{request.Key}' already exists."),
                _ => BadRequest(result.Code.ToString())
            };
        }

        [HttpPut("definitions/{id:int}")]
        public async Task<ActionResult<ItemDefinition>> UpdateDefinition(int id, UpdateItemDefinitionRequest request)
        {
            var result = await inventoryService.UpdateDefinitionAsync(id, request);
            return result.Code switch
            {
                InventoryOpCode.Success => Ok(result.Value),
                InventoryOpCode.DefinitionNotFound => NotFound("Definition not found."),
                _ => BadRequest(result.Code.ToString())
            };
        }

        [HttpPatch("definitions/{id:int}/active")]
        public async Task<IActionResult> SetActive(int id, [FromQuery] bool isActive)
        {
            var code = await inventoryService.SetActiveAsync(id, isActive);
            return code switch
            {
                InventoryOpCode.Success => NoContent(),
                InventoryOpCode.DefinitionNotFound => NotFound("Definition not found."),
                _ => BadRequest(code.ToString())
            };
        }

        [HttpPost("grant")]
        public async Task<IActionResult> Grant(GrantItemRequest request)
        {
            if (request.Quantity < 1)
                return BadRequest("Quantity must be at least 1.");

            InventoryOpCode code;
            if (request.All)
                code = await inventoryService.GrantItemToAllAsync(request.DefinitionKey, request.Quantity);
            else if (request.UserId is { } userId)
                code = await inventoryService.GrantItemAsync(userId, request.DefinitionKey, request.Quantity);
            else
                return BadRequest("Provide a userId or set all=true.");

            return code switch
            {
                InventoryOpCode.Success => NoContent(),
                InventoryOpCode.DefinitionNotFound => NotFound($"Definition '{request.DefinitionKey}' not found."),
                _ => BadRequest(code.ToString())
            };
        }
    }
}
