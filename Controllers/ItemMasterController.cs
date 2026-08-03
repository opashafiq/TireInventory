using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;
using static TireInventory.Helpers.CommonFunctions;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ItemMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/ItemMaster
        [HttpGet]
        //public async Task<ActionResult<IEnumerable<ItemMaster>>> GetItemMasters()
        //{
        //    return await _context.ItemMasters.ToListAsync();
        //}

        public async Task<ActionResult<IEnumerable<ItemMasterDto>>> GetItemMasters()
        {
            var list = await (from im in _context.ItemMasters
                              join dep in _context.Departments
                                  on im.tbim_ItemCategoryId equals dep.Id
                              join dis in _context.Distributors
                                    on im.tbim_DistributorId equals dis.Id
                              join loc in _context.LocationDetails
                                    on im.tbim_LocationId equals loc.Id
                              select new ItemMasterDto
                              {
                                  Id = im.Id,
                                  tbim_ItemCategoryId = im.tbim_ItemCategoryId,
                                  tbim_Size = im.tbim_Size,
                                  tbim_Brand = im.tbim_Brand,
                                  tbim_Series = im.tbim_Series,
                                  tbim_Bolt = im.tbim_Bolt,
                                  tbim_HoleS = im.tbim_HoleS,
                                  tbim_Zone = im.tbim_Zone,
                                  tbim_Qty = im.tbim_Qty,
                                  tbim_QtyOp = im.tbim_QtyOp,
                                  tbim_Code = im.tbim_Code,
                                  tbim_CodeTOT = im.tbim_CodeTOT,
                                  tbim_DistributorId = im.tbim_DistributorId,
                                  tbim_OURP = im.tbim_OURP,
                                  tbim_LocationId = im.tbim_LocationId,
                                  DepartmentName = dep.Tbid_DepartmentName,
                                  DistributorName = dis.Name,
                                  LocationName = loc.tbld_LocationName,
                                  tbim_ThrashDate = im.tbim_ThrashDate,
                                  UserName = im.UserName,
                                  SetDate = im.SetDate

                              })
                             .ToListAsync();

            return Ok(list);
        }

        // GET: api/ItemMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemMasterDto>> GetItemMaster(long id)
        {
            var dto = await (from im in _context.ItemMasters
                             join dep in _context.Departments
                                 on im.tbim_ItemCategoryId equals dep.Id
                             join dis in _context.Distributors
                                   on im.tbim_DistributorId equals dis.Id
                             join loc in _context.LocationDetails
                                   on im.tbim_LocationId equals loc.Id
                             where im.Id == id
                             select new ItemMasterDto
                             {
                                 Id = im.Id,
                                 tbim_ItemCategoryId = im.tbim_ItemCategoryId,
                                 tbim_Size = im.tbim_Size,
                                 tbim_Brand = im.tbim_Brand,
                                 tbim_Series = im.tbim_Series,
                                 tbim_Bolt = im.tbim_Bolt,
                                 tbim_HoleS = im.tbim_HoleS,
                                 tbim_Zone = im.tbim_Zone,
                                 tbim_Qty = im.tbim_Qty,
                                 tbim_QtyOp = im.tbim_QtyOp,
                                 tbim_Code = im.tbim_Code,
                                 tbim_CodeTOT = im.tbim_CodeTOT,
                                 tbim_DistributorId = im.tbim_DistributorId,
                                 tbim_OURP = im.tbim_OURP,
                                 tbim_LocationId = im.tbim_LocationId,
                                 DepartmentName = dep.Tbid_DepartmentName,
                                 DistributorName = dis.Name,
                                 LocationName = loc.tbld_LocationName,
                                 tbim_ThrashDate = im.tbim_ThrashDate,
                                 UserName = im.UserName,
                                 SetDate = im.SetDate
                             })
                            .FirstOrDefaultAsync();

            if (dto == null) return NotFound();
            return Ok(dto);
        }
        
        
        // GET: api/ItemMaster/getbycategory/5
        [HttpGet("getbycategory/{id}")]
        public async Task<ActionResult<IEnumerable<ItemMasterDto>>> GetByCategory(long id)
        {
            var list = await (from im in _context.ItemMasters
                              join dep in _context.Departments
                                  on im.tbim_ItemCategoryId equals dep.Id
                              join dis in _context.Distributors
                                    on im.tbim_DistributorId equals dis.Id
                              join loc in _context.LocationDetails
                                    on im.tbim_LocationId equals loc.Id
                              where im.tbim_ItemCategoryId==id
                              select new ItemMasterDto
                              {
                                  Id = im.Id,
                                  tbim_ItemCategoryId = im.tbim_ItemCategoryId,
                                  tbim_Size = im.tbim_Size,
                                  tbim_Brand = im.tbim_Brand,
                                  tbim_Series = im.tbim_Series,
                                  tbim_Bolt = im.tbim_Bolt,
                                  tbim_HoleS = im.tbim_HoleS,
                                  tbim_Zone = im.tbim_Zone,
                                  tbim_Qty = im.tbim_Qty,
                                  tbim_QtyOp = im.tbim_QtyOp,
                                  tbim_Code = im.tbim_Code,
                                  tbim_CodeTOT = im.tbim_CodeTOT,
                                  tbim_DistributorId = im.tbim_DistributorId,
                                  tbim_OURP = im.tbim_OURP,
                                  tbim_LocationId = im.tbim_LocationId,
                                  DepartmentName = dep.Tbid_DepartmentName,
                                  DistributorName = dis.Name,
                                  LocationName = loc.tbld_LocationName,
                                  tbim_ThrashDate = im.tbim_ThrashDate,
                                  UserName = im.UserName,
                                  SetDate = im.SetDate

                              })
                            .ToListAsync();

            return Ok(list);
        }


        // PUT: api/ItemMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemMaster(long id, ItemMaster itemMaster)
        {
            if (id != itemMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(itemMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemMasterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ItemMaster
        [HttpPost]
        public async Task<ActionResult<ItemMasterDto>> PostItemMaster(ItemMaster itemMaster)
        {
            _context.ItemMasters.Add(itemMaster);
            await _context.SaveChangesAsync();

            return await GetItemMaster(itemMaster.Id);
            //return CreatedAtAction("GetItemMaster", new { id = itemMaster.Id }, itemMaster);
        }

        // DELETE: api/ItemMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemMaster(long id)
        {
            var itemMaster = await _context.ItemMasters.FindAsync(id);
            if (itemMaster == null)
            {
                return NotFound();
            }

            _context.ItemMasters.Remove(itemMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/ItemMaster/bulk-import
        [HttpPost("bulk-import")]
        public async Task<IActionResult> BulkImportItems(
                [FromBody] List<ItemMaster> items,
                [FromQuery] bool skipErrors = false)
        {
            if (items == null || !items.Any())
            {
                return BadRequest(new { message = "No item data provided." });
            }

            var result = new BulkImportResultDto();
            ItemMaster existingItem = new ItemMaster();
            var itemsToInsert = new List<ItemMaster>();
            var itemsToUpdate = new List<ItemMaster>();
            int itemCount = 0;

            // 1. Process and validate incoming rows
            for (int i = 0; i < items.Count; i++)
            {
                var dto = items[i];
                int rowNumber = i + 1;
                //itemCount = _context.ItemMasters.Count(im => 
                //im.tbim_ItemCategoryId == dto.tbim_ItemCategoryId &&
                //im.tbim_Size == dto.tbim_Size &&
                //im.tbim_Brand == dto.tbim_Brand &&
                //im.tbim_Series == dto.tbim_Series &&
                //im.tbim_Bolt == dto.tbim_Bolt &&
                //im.tbim_HoleS == dto.tbim_HoleS 
                //);

                existingItem=_context.ItemMasters.Where(im =>
                im.tbim_ItemCategoryId == dto.tbim_ItemCategoryId &&
                im.tbim_Size == dto.tbim_Size &&
                im.tbim_Brand == dto.tbim_Brand &&
                im.tbim_Series == dto.tbim_Series &&
                im.tbim_Bolt == dto.tbim_Bolt &&
                im.tbim_HoleS == dto.tbim_HoleS 
                ).FirstOrDefault();

                // Example validation checks
                //if (string.IsNullOrWhiteSpace(dto.ItemDescription))
                //{
                //    result.Errors.Add($"Row {rowNumber}: Item description is required.");
                //    continue;
                //}

                //if (dto.UnitPrice < 0)
                //{
                //    result.Errors.Add($"Row {rowNumber}: Unit price cannot be negative.");
                //    continue;
                //}

                if (existingItem!=null) // Update
                {
                    existingItem.tbim_Qty = dto.tbim_Qty;
                    existingItem.tbim_QtyOp = dto.tbim_QtyOp;
                    existingItem.tbim_Code = dto.tbim_Code;
                    existingItem.tbim_CodeTOT = dto.tbim_CodeTOT;
                    existingItem.tbim_OURP = dto.tbim_OURP;
                    existingItem.tbim_ThrashDate = dto.tbim_ThrashDate;
                    existingItem.UserName = dto.UserName;
                    existingItem.SetDate = DateTime.UtcNow; // or any other logic for setting the date
                    existingItem.tbim_LocationId = dto.tbim_LocationId;
                    existingItem.tbim_Zone = dto.tbim_Zone;
                    existingItem.tbim_DistributorId = dto.tbim_DistributorId;
                    //existingItem.tbim_Series = dto.tbim_Series;
                    //existingItem.tbim_Bolt = dto.tbim_Bolt;
                    //existingItem.tbim_HoleS = dto.tbim_HoleS;
                    //existingItem.tbim_Brand = dto.tbim_Brand;
                    //existingItem.tbim_Size = dto.tbim_Size;
                    //existingItem.tbim_ItemCategoryId = dto.tbim_ItemCategoryId;

                    itemsToUpdate.Add(existingItem);

                    //result.Errors.Add($"Row {rowNumber}: Duplicate item found based on category, size, brand, series, bolt, and holes.");
                    //continue;
                }
                else // Insert 
                {
                    itemsToInsert.Add(new ItemMaster
                    {
                        tbim_ItemCategoryId = dto.tbim_ItemCategoryId,
                        tbim_Size = dto.tbim_Size,
                        tbim_Brand = dto.tbim_Brand,
                        tbim_Series = dto.tbim_Series,
                        tbim_Bolt = dto.tbim_Bolt,
                        tbim_HoleS = dto.tbim_HoleS,
                        tbim_Zone = dto.tbim_Zone,
                        tbim_Qty = dto.tbim_Qty,
                        tbim_QtyOp = dto.tbim_QtyOp,
                        tbim_Code = dto.tbim_Code,
                        tbim_CodeTOT = dto.tbim_CodeTOT,
                        tbim_DistributorId = dto.tbim_DistributorId,
                        tbim_OURP = dto.tbim_OURP,
                        tbim_ThrashDate = dto.tbim_ThrashDate,
                        UserName = dto.UserName,
                        SetDate = DateTime.UtcNow, // or any other logic for setting the date
                        tbim_LocationId = dto.tbim_LocationId
                    });
                }
            }

            // 2. If errors exist and skipErrors is false, reject the batch
            if (result.Errors.Any() && !skipErrors)
            {
                result.ErrorCount = result.Errors.Count;
                return BadRequest(result);
            }

            // 3. Perform database insertion within a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ItemMasters.AddRangeAsync(itemsToInsert);
                _context.ItemMasters.UpdateRange(itemsToUpdate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.SuccessCount = itemsToInsert.Count;
                result.ErrorCount = result.Errors.Count;

                return Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred during bulk import.", error = ex.InnerException.Message });
            }
        }
        // POST: api/ItemMaster/bulk-import-cat
        [HttpPost("bulk-import-cat")]
        public async Task<IActionResult> BulkImportItemsCat(
                [FromBody] List<ItemMasterBulkDto> items,
                [FromQuery] bool skipErrors = false)
        {
            if (items == null || !items.Any())
            {
                return BadRequest(new { message = "No item data provided." });
            }

            try
            {
                // -------------------------------------------------------------
                // STEP 1: Extract distinct, non-empty Distributor Names from DTOs
                // -------------------------------------------------------------
                var dtoDistributorNames = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.tbim_DistributorName))
                    .Select(x => x.tbim_DistributorName.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!dtoDistributorNames.Any())
                {
                    // Early return or handle case where no distributors are passed
                }

                // -------------------------------------------------------------
                // STEP 2: Fetch ALL existing distributors into memory 
                // (Or match via EF Core Contains if table is very large)
                // -------------------------------------------------------------

                // Fetch existing records into memory to prevent EF Core SQL 'WITH' CTE generation
                var allDbDistributors = await _context.Distributors.ToListAsync();

                // Perform case-insensitive matching locally in C#
                var existingDistributors = allDbDistributors
                    .Where(d => dtoDistributorNames.Contains(d.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                var existingDistributorNames = existingDistributors
                    .Select(c => c.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // -------------------------------------------------------------
                // STEP 3: Identify missing distributors & insert them
                // -------------------------------------------------------------
                var missingDistributorNames = dtoDistributorNames
                    .Where(name => !existingDistributorNames.Contains(name))
                    .ToList();

                if (missingDistributorNames.Any())
                {
                    var currentUserName = items.FirstOrDefault(x => !string.IsNullOrEmpty(x.UserName))?.UserName;

                    var newDistributors = missingDistributorNames.Select(name => new Distributors
                    {
                        Name = name,
                        Address = null,
                        UserName = currentUserName,
                        SetDate = DateTime.UtcNow
                    }).ToList();

                    await _context.Distributors.AddRangeAsync(newDistributors);
                    await _context.SaveChangesAsync();

                    // Merge newly created entities into existing list
                    existingDistributors.AddRange(newDistributors);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during checking and creating new Distributors.", error = ex.InnerException?.Message ?? ex.Message });
            }



            var result = new BulkImportResultDto();
            return Ok(result);


            ItemMaster existingItem = new ItemMaster();
            var itemsToInsert = new List<ItemMaster>();
            var itemsToUpdate = new List<ItemMaster>();
            int itemCount = 0;

            // 1. Process and validate incoming rows
            for (int i = 0; i < items.Count; i++)
            {
                var dto = items[i];
                int rowNumber = i + 1;
                //itemCount = _context.ItemMasters.Count(im => 
                //im.tbim_ItemCategoryId == dto.tbim_ItemCategoryId &&
                //im.tbim_Size == dto.tbim_Size &&
                //im.tbim_Brand == dto.tbim_Brand &&
                //im.tbim_Series == dto.tbim_Series &&
                //im.tbim_Bolt == dto.tbim_Bolt &&
                //im.tbim_HoleS == dto.tbim_HoleS 
                //);

                existingItem=_context.ItemMasters.Where(im =>
                im.tbim_ItemCategoryId == dto.tbim_ItemCategoryId &&
                im.tbim_Size == dto.tbim_Size &&
                im.tbim_Brand == dto.tbim_Brand &&
                im.tbim_Series == dto.tbim_Series &&
                im.tbim_Bolt == dto.tbim_Bolt &&
                im.tbim_HoleS == dto.tbim_HoleS 
                ).FirstOrDefault();

                // Example validation checks
                //if (string.IsNullOrWhiteSpace(dto.ItemDescription))
                //{
                //    result.Errors.Add($"Row {rowNumber}: Item description is required.");
                //    continue;
                //}

                //if (dto.UnitPrice < 0)
                //{
                //    result.Errors.Add($"Row {rowNumber}: Unit price cannot be negative.");
                //    continue;
                //}

                if (existingItem!=null) // Update
                {
                    existingItem.tbim_Qty = dto.tbim_Qty;
                    existingItem.tbim_QtyOp = dto.tbim_QtyOp;
                    existingItem.tbim_Code = dto.tbim_Code;
                    existingItem.tbim_CodeTOT = dto.tbim_CodeTOT;
                    existingItem.tbim_OURP = dto.tbim_OURP;
                    existingItem.tbim_ThrashDate = dto.tbim_ThrashDate;
                    existingItem.UserName = dto.UserName;
                    existingItem.SetDate = DateTime.UtcNow; // or any other logic for setting the date
                    existingItem.tbim_LocationId = dto.tbim_LocationId;
                    existingItem.tbim_Zone = dto.tbim_Zone;
                    ////existingItem.tbim_DistributorId = dto.tbim_DistributorId;
                    //existingItem.tbim_Series = dto.tbim_Series;
                    //existingItem.tbim_Bolt = dto.tbim_Bolt;
                    //existingItem.tbim_HoleS = dto.tbim_HoleS;
                    //existingItem.tbim_Brand = dto.tbim_Brand;
                    //existingItem.tbim_Size = dto.tbim_Size;
                    //existingItem.tbim_ItemCategoryId = dto.tbim_ItemCategoryId;

                    itemsToUpdate.Add(existingItem);

                    //result.Errors.Add($"Row {rowNumber}: Duplicate item found based on category, size, brand, series, bolt, and holes.");
                    //continue;
                }
                else // Insert 
                {
                    itemsToInsert.Add(new ItemMaster
                    {
                        tbim_ItemCategoryId = dto.tbim_ItemCategoryId,
                        tbim_Size = dto.tbim_Size,
                        tbim_Brand = dto.tbim_Brand,
                        tbim_Series = dto.tbim_Series,
                        tbim_Bolt = dto.tbim_Bolt,
                        tbim_HoleS = dto.tbim_HoleS,
                        tbim_Zone = dto.tbim_Zone,
                        tbim_Qty = dto.tbim_Qty,
                        tbim_QtyOp = dto.tbim_QtyOp,
                        tbim_Code = dto.tbim_Code,
                        tbim_CodeTOT = dto.tbim_CodeTOT,
                        ////tbim_DistributorId = dto.tbim_DistributorId,
                        tbim_OURP = dto.tbim_OURP,
                        tbim_ThrashDate = dto.tbim_ThrashDate,
                        UserName = dto.UserName,
                        SetDate = DateTime.UtcNow, // or any other logic for setting the date
                        tbim_LocationId = dto.tbim_LocationId
                    });
                }
            }

            // 2. If errors exist and skipErrors is false, reject the batch
            if (result.Errors.Any() && !skipErrors)
            {
                result.ErrorCount = result.Errors.Count;
                return BadRequest(result);
            }

            // 3. Perform database insertion within a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ItemMasters.AddRangeAsync(itemsToInsert);
                _context.ItemMasters.UpdateRange(itemsToUpdate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.SuccessCount = itemsToInsert.Count;
                result.ErrorCount = result.Errors.Count;

                return Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred during bulk import.", error = ex.InnerException.Message });
            }
        }

        private bool ItemMasterExists(long id)
        {
            return _context.ItemMasters.Any(e => e.Id == id);
        }
    }
}