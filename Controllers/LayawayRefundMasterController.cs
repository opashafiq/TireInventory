using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class LayawayRefundMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayRefundMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayRefundMaster
        [HttpGet]
        public async Task<ActionResult<PagedLayawayRefundResponseDto>> GetLayawayRefundMasters(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] long? invoiceId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
            )
        {
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            // Start with a base, un-executed query
            var query = _context.LayawayRefundMasters.AsNoTracking();

            // --- Dynamic Filtering Blocks ---
            if (startDate.HasValue)
            {
                query = query.Where(o => o.Layaway_tbim_InvDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var inclusiveEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.Layaway_tbim_InvDate <= inclusiveEndDate);
            }

            if (invoiceId.HasValue)
            {
                query = query.Where(o => o.Id == invoiceId.Value);
            }

            // --- CRUCIAL FIX: Calculate total count based on the FILTERED query parameters ---
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var masters = await query
                .Include(m => m.LayawayRefundDetails)
                .Include(m => m.LayawayRefundPayments)
                .OrderByDescending(m => m.tbirm_LayawayRefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<CreateLayawayRefundDto>();
            foreach (var m in masters)
            {
                dtos.Add(MapToCreateLayawayRefundDto(m));
            }

            // 3. Build and return the structured response object directly matching your layout
            var response = new PagedLayawayRefundResponseDto
            {
                Items = dtos,
                TotalCount = totalRecords,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return Ok(response);
        }

        // GET: api/LayawayRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreateLayawayRefundDto>> GetLayawayRefundMaster(long id)
        {
            var master = await _context.LayawayRefundMasters
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (master == null) return NotFound();
            return Ok(MapToCreateLayawayRefundDto(master));
        }

        // POST: api/LayawayRefundMaster/EditLayawayRefund
        [HttpPost("EditLayawayRefund")]
        public async Task<IActionResult> EditLayawayRefund(long id, CreateLayawayRefundDto createDto)
        {
            if (createDto == null) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var refundMaster = await _context.LayawayRefundMasters
                    .Include(m => m.LayawayRefundDetails)
                    .Include(m => m.LayawayRefundPayments)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (refundMaster == null) return NotFound();

                UpdateLayawayRefundMaster(refundMaster, createDto.layawayRefundMasterDto ?? new LayawayRefundMasterDto());
                _context.Entry(refundMaster).State = EntityState.Modified;

                ProcessLayawayRefundDetailsUpsert(refundMaster, createDto.layawayRefundDetailsDto ?? new List<LayawayRefundDetailsDto>());
                ProcessLayawayRefundPaymentsUpsert(refundMaster, createDto.layawayRefundPaymentsDto ?? new List<LayawayRefundPaymentsDto>());

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "Layaway refund adjusted successfully", RefundId = id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the layaway refund.");
            }
        }

        // POST: api/LayawayRefundMaster/CreateLayawayRefund
        [HttpPost("CreateLayawayRefund")]
        public async Task<ActionResult<CreateLayawayRefundDto>> CreateLayawayRefund(CreateLayawayRefundDto createDto)
        {
            if (createDto == null) return BadRequest();

            var refundMaster = MapToLayawayRefundMaster(createDto.layawayRefundMasterDto ?? new LayawayRefundMasterDto());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.LayawayRefundMasters.Add(refundMaster);
                await _context.SaveChangesAsync();

                var details = MapToLayawayRefundDetails(createDto.layawayRefundDetailsDto ?? new List<LayawayRefundDetailsDto>());
                foreach (var d in details)
                {
                    d.tbird_Layaway_RefundId = refundMaster.Id;
                    d.Id = 0;
                    d.tbird_Layaway_Refund = null;

                    if (d.tbird_ItemId.HasValue)
                    {
                        var itemMaster = await _context.ItemMasters.FindAsync(d.tbird_ItemId.Value);
                        if (itemMaster != null)
                        {
                            try
                            {
                                d.tbird_ItemCategory = (int?)itemMaster.tbim_ItemCategoryId;
                            }
                            catch
                            {
                                d.tbird_ItemCategory = null;
                            }

                            var dept = await _context.Departments.FindAsync(itemMaster.tbim_ItemCategoryId);
                            d.tbird_DepartmentName = dept?.Tbid_DepartmentName;

                            d.tbird_Size = itemMaster.tbim_Size;
                            d.tbird_Brand = itemMaster.tbim_Brand;
                            d.tbird_Series = itemMaster.tbim_Series;
                            d.tbird_Bolt = itemMaster.tbim_Bolt;
                            d.tbird_HoleS = itemMaster.tbim_HoleS;
                            d.tbird_Zone = itemMaster.tbim_Zone;
                            d.tbird_DistributorId = itemMaster.tbim_DistributorId;

                            if (itemMaster.tbim_DistributorId.HasValue)
                            {
                                var distributor = await _context.Distributors.FindAsync(itemMaster.tbim_DistributorId.Value);
                                d.tbird_DistributorName = distributor?.Name;
                            }
                        }
                    }
                }

                if (details.Count > 0)
                {
                    _context.LayawayRefundDetails.AddRange(details);
                }

                var payments = MapToLayawayRefundPayments(createDto.layawayRefundPaymentsDto ?? new List<LayawayRefundPaymentsDto>());
                foreach (var p in payments)
                {
                    p.tbirp_Layaway_RefundId = refundMaster.Id;
                    p.Id = 0;
                    p.tbirp_Layaway_Refund = null;
                }

                if (payments.Count > 0)
                {
                    _context.LayawayRefundPayments.AddRange(payments);
                }

                await _context.SaveChangesAsync();

                var created = await _context.LayawayRefundMasters
                    .Include(m => m.LayawayRefundDetails)
                    .Include(m => m.LayawayRefundPayments)
                    .FirstOrDefaultAsync(m => m.Id == refundMaster.Id);

                await transaction.CommitAsync();
                return await GetLayawayRefundMaster(refundMaster.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Critical database transaction failure while creating the layaway refund.");
            }
        }

        // DELETE: api/LayawayRefundMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayRefundMaster(long id)
        {
            var layawayRefundMaster = await _context.LayawayRefundMasters.FindAsync(id);
            if (layawayRefundMaster == null) return NotFound();

            _context.LayawayRefundMasters.Remove(layawayRefundMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LayawayRefundMasterExists(long id)
        {
            return _context.LayawayRefundMasters.Any(e => e.Id == id);
        }

        // Mapping helpers
        private CreateLayawayRefundDto MapToCreateLayawayRefundDto(LayawayRefundMaster lm)
        {
            return new CreateLayawayRefundDto
            {
                layawayRefundMasterDto = MapToLayawayRefundMasterDto(lm),
                layawayRefundDetailsDto = MapToLayawayRefundDetailsDto(lm.Id),
                layawayRefundPaymentsDto = MapToLayawayRefundPaymentsDto(lm.Id)
            };
        }

        private LayawayRefundMasterDto MapToLayawayRefundMasterDto(LayawayRefundMaster lm)
        {
            return new LayawayRefundMasterDto
            {
                Id = lm.Id,
                tbirm_LayawayRefundIdRad = lm.tbirm_LayawayRefundIdRad,
                tbirm_LayawayRefundDate = lm.tbirm_LayawayRefundDate,
                tbirm_RefundType = lm.tbirm_RefundType,
                tbirm_SubTotal = lm.tbirm_SubTotal,
                tbirm_SaleTax = lm.tbirm_SaleTax,
                tbirm_Labour = lm.tbirm_Labour,
                tbirm_DisPer = lm.tbirm_DisPer,
                tbirm_DisAmt = lm.tbirm_DisAmt,
                tbirm_Total = lm.tbirm_Total,
                tbirm_RefundAmt = lm.tbirm_RefundAmt,
                tbirm_AdjAmt = lm.tbirm_AdjAmt,
                tbirm_Note = lm.tbirm_Note,
                tbirm_Delinfo = lm.tbirm_Delinfo,
                tbirm_Item_Delete_after_Layaway_Refund_Create = lm.tbirm_Item_Delete_after_Layaway_Refund_Create,
                UserName = lm.UserName,
                SetDate = lm.SetDate,
                Layaway_tbim_InvoiceId = lm.Layaway_tbim_InvoiceId,
                Layaway_tbim_InvoiceIdRad = lm.Layaway_tbim_InvoiceIdRad,
                Layaway_tbim_Phone = lm.Layaway_tbim_Phone,
                Layaway_tbim_InvDate = lm.Layaway_tbim_InvDate,
                Layaway_tbim_Name = lm.Layaway_tbim_Name,
                Layaway_tbim_TaxId = lm.Layaway_tbim_TaxId,
                Layaway_tbim_VehicleMake = lm.Layaway_tbim_VehicleMake,
                Layaway_tbim_Model = lm.Layaway_tbim_Model,
                Layaway_tbim_Year = lm.Layaway_tbim_Year,
                Layaway_tbim_Odometer = lm.Layaway_tbim_Odometer,
                Layaway_tbim_TreadDepth = lm.Layaway_tbim_TreadDepth,
                Layaway_tbim_License = lm.Layaway_tbim_License,
                Layaway_tbim_SubTotal = lm.Layaway_tbim_SubTotal,
                Layaway_tbim_SaleTax = lm.Layaway_tbim_SaleTax,
                Layaway_tbim_Labour = lm.Layaway_tbim_Labour,
                Layaway_tbim_DisPer = lm.Layaway_tbim_DisPer,
                Layaway_tbim_DisAmt = lm.Layaway_tbim_DisAmt,
                Layaway_tbim_Total = lm.Layaway_tbim_Total,
                Layaway_tbim_PaidAmt = lm.Layaway_tbim_PaidAmt,
                Layaway_tbim_AdjAmt = lm.Layaway_tbim_AdjAmt,
                Layaway_tbim_AdjTotal = lm.Layaway_tbim_AdjTotal,
                Layaway_tbim_PayInfo = lm.Layaway_tbim_PayInfo,
                Layaway_tbim_Note = lm.Layaway_tbim_Note,
                Layaway_tbim_Delinfo = lm.Layaway_tbim_Delinfo,
                Layaway_tbim_CompanyName = lm.Layaway_tbim_CompanyName,
                Layaway_tbim_CompanyAddress = lm.Layaway_tbim_CompanyAddress,
                Layaway_tbim_Item_Delete_after_Layaway_Create = lm.Layaway_tbim_Item_Delete_after_Layaway_Create,
                Layaway_tbim_Left_Front = lm.Layaway_tbim_Left_Front,
                Layaway_tbim_Right_Front = lm.Layaway_tbim_Right_Front,
                Layaway_tbim_Left_Rear = lm.Layaway_tbim_Left_Rear,
                Layaway_tbim_Right_Rear = lm.Layaway_tbim_Right_Rear,
                Layaway_tbim_EmailAddress = lm.Layaway_tbim_EmailAddress,
                Layaway_tbim_IDNo = lm.Layaway_tbim_IDNo,
                // resolved original layaway info
                OriginalLayawayName = lm.Layaway_tbim_Name ?? string.Empty,
                OriginalLayawayDate = lm.Layaway_tbim_InvDate
            };
        }

        private List<LayawayRefundDetailsDto> MapToLayawayRefundDetailsDto(long id)
        {
            var list = _context.LayawayRefundDetails
                .Where(d => d.tbird_Layaway_RefundId == id)
                .Select(d => new LayawayRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_Layaway_RefundId = d.tbird_Layaway_RefundId,
                    tbird_ItemId = d.tbird_ItemId,
                    tbird_ItemCategory = d.tbird_ItemCategory,
                    tbird_DepartmentName = d.tbird_DepartmentName,
                    tbird_Size = d.tbird_Size,
                    tbird_Brand = d.tbird_Brand,
                    tbird_Series = d.tbird_Series,
                    tbird_Bolt = d.tbird_Bolt,
                    tbird_HoleS = d.tbird_HoleS,
                    tbird_Zone = d.tbird_Zone,
                    tbird_DistributorId = d.tbird_DistributorId,
                    tbird_DistributorName = d.tbird_DistributorName,
                    tbird_Qty = d.tbird_Qty,
                    tbird_Layaway_Qty = d.tbird_Layaway_Qty,
                    tbird_Layaway_Qty_LineTotal = d.tbird_Layaway_Qty_LineTotal,
                    tbird_Layaway_Qty_TaxAmt = d.tbird_Layaway_Qty_TaxAmt,
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
                    tbird_TaxRate = d.tbird_TaxRate,
                    tbird_TaxAmt = d.tbird_TaxAmt,
                    ItemDepartmentName = d.tbird_Item != null && d.tbird_Item.tbim_ItemCategory != null
                        ? d.tbird_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbird_Item != null && d.tbird_Item.tbim_Distributor != null
                        ? d.tbird_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbird_Item != null && d.tbird_Item.tbim_Location != null
                        ? d.tbird_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbird_Item != null
                        ? (d.tbird_Item.tbim_Brand + " " + d.tbird_Item.tbim_Size).Trim() : string.Empty
                })
                .ToList();

            return list;
        }

        private List<LayawayRefundPaymentsDto> MapToLayawayRefundPaymentsDto(long id)
        {
            var list = (from p in _context.LayawayRefundPayments
                        join rm in _context.RefundMethodNames
                            on p.tbirp_RefundMethodId equals rm.Id into gj
                        from rm in gj.DefaultIfEmpty()
                        where p.tbirp_Layaway_RefundId == id
                        select new LayawayRefundPaymentsDto
                        {
                            Id = p.Id,
                            tbirp_Layaway_RefundId = p.tbirp_Layaway_RefundId,
                            tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                            tbirp_RefundAmt = p.tbirp_RefundAmt,
                            tbirp_Date = p.tbirp_Date,
                            RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                        })
                        .ToList();

            return list;
        }

        private LayawayRefundMaster MapToLayawayRefundMaster(LayawayRefundMasterDto dto)
        {
            return new LayawayRefundMaster
            {
                Id = dto.Id,
                tbirm_LayawayRefundIdRad = dto.tbirm_LayawayRefundIdRad,
                tbirm_LayawayRefundDate = dto.tbirm_LayawayRefundDate,
                tbirm_RefundType = dto.tbirm_RefundType,
                tbirm_SubTotal = dto.tbirm_SubTotal,
                tbirm_SaleTax = dto.tbirm_SaleTax,
                tbirm_Labour = dto.tbirm_Labour,
                tbirm_DisPer = dto.tbirm_DisPer,
                tbirm_DisAmt = dto.tbirm_DisAmt,
                tbirm_Total = dto.tbirm_Total,
                tbirm_RefundAmt = dto.tbirm_RefundAmt,
                tbirm_AdjAmt = dto.tbirm_AdjAmt,
                tbirm_Note = dto.tbirm_Note,
                tbirm_Delinfo = dto.tbirm_Delinfo,
                tbirm_Item_Delete_after_Layaway_Refund_Create = dto.tbirm_Item_Delete_after_Layaway_Refund_Create,
                UserName = dto.UserName,
                SetDate = dto.SetDate,
                Layaway_tbim_InvoiceId = dto.Layaway_tbim_InvoiceId,
                Layaway_tbim_InvoiceIdRad = dto.Layaway_tbim_InvoiceIdRad,
                Layaway_tbim_Phone = dto.Layaway_tbim_Phone,
                Layaway_tbim_InvDate = dto.Layaway_tbim_InvDate,
                Layaway_tbim_Name = dto.Layaway_tbim_Name,
                Layaway_tbim_TaxId = dto.Layaway_tbim_TaxId,
                Layaway_tbim_VehicleMake = dto.Layaway_tbim_VehicleMake,
                Layaway_tbim_Model = dto.Layaway_tbim_Model,
                Layaway_tbim_Year = dto.Layaway_tbim_Year,
                Layaway_tbim_Odometer = dto.Layaway_tbim_Odometer,
                Layaway_tbim_TreadDepth = dto.Layaway_tbim_TreadDepth,
                Layaway_tbim_License = dto.Layaway_tbim_License,
                Layaway_tbim_SubTotal = dto.Layaway_tbim_SubTotal,
                Layaway_tbim_SaleTax = dto.Layaway_tbim_SaleTax,
                Layaway_tbim_Labour = dto.Layaway_tbim_Labour,
                Layaway_tbim_DisPer = dto.Layaway_tbim_DisPer,
                Layaway_tbim_DisAmt = dto.Layaway_tbim_DisAmt,
                Layaway_tbim_Total = dto.Layaway_tbim_Total,
                Layaway_tbim_PaidAmt = dto.Layaway_tbim_PaidAmt,
                Layaway_tbim_AdjAmt = dto.Layaway_tbim_AdjAmt,
                Layaway_tbim_AdjTotal = dto.Layaway_tbim_AdjTotal,
                Layaway_tbim_PayInfo = dto.Layaway_tbim_PayInfo,
                Layaway_tbim_Note = dto.Layaway_tbim_Note,
                Layaway_tbim_Delinfo = dto.Layaway_tbim_Delinfo,
                Layaway_tbim_CompanyName = dto.Layaway_tbim_CompanyName,
                Layaway_tbim_CompanyAddress = dto.Layaway_tbim_CompanyAddress,
                Layaway_tbim_Item_Delete_after_Layaway_Create = dto.Layaway_tbim_Item_Delete_after_Layaway_Create,
                Layaway_tbim_Left_Front = dto.Layaway_tbim_Left_Front,
                Layaway_tbim_Right_Front = dto.Layaway_tbim_Right_Front,
                Layaway_tbim_Left_Rear = dto.Layaway_tbim_Left_Rear,
                Layaway_tbim_Right_Rear = dto.Layaway_tbim_Right_Rear,
                Layaway_tbim_EmailAddress = dto.Layaway_tbim_EmailAddress,
                Layaway_tbim_IDNo = dto.Layaway_tbim_IDNo
            };
        }

        private List<LayawayRefundDetails> MapToLayawayRefundDetails(List<LayawayRefundDetailsDto> dtos)
        {
            var list = new List<LayawayRefundDetails>();
            foreach (var dto in dtos)
            {
                var detail = new LayawayRefundDetails
                {
                    Id = dto.Id,
                    tbird_Layaway_RefundId = dto.tbird_Layaway_RefundId,
                    tbird_ItemId = dto.tbird_ItemId,
                    tbird_ItemCategory = dto.tbird_ItemCategory,
                    tbird_DepartmentName = dto.tbird_DepartmentName,
                    tbird_Size = dto.tbird_Size,
                    tbird_Brand = dto.tbird_Brand,
                    tbird_Series = dto.tbird_Series,
                    tbird_Bolt = dto.tbird_Bolt,
                    tbird_HoleS = dto.tbird_HoleS,
                    tbird_Zone = dto.tbird_Zone,
                    tbird_DistributorId = dto.tbird_DistributorId,
                    tbird_DistributorName = dto.tbird_DistributorName,
                    tbird_Qty = dto.tbird_Qty,
                    tbird_Layaway_Qty = dto.tbird_Layaway_Qty,
                    tbird_Layaway_Qty_LineTotal = dto.tbird_Layaway_Qty_LineTotal,
                    tbird_Layaway_Qty_TaxAmt = dto.tbird_Layaway_Qty_TaxAmt,
                    tbird_Taxable = dto.tbird_Taxable,
                    tbird_UnitPrice = dto.tbird_UnitPrice,
                    tbird_LineTotal = dto.tbird_LineTotal,
                    tbird_TaxRate = dto.tbird_TaxRate,
                    tbird_TaxAmt = dto.tbird_TaxAmt
                };
                list.Add(detail);
            }
            return list;
        }

        private List<LayawayRefundPayments> MapToLayawayRefundPayments(List<LayawayRefundPaymentsDto> dtos)
        {
            var list = new List<LayawayRefundPayments>();
            foreach (var dto in dtos)
            {
                var payment = new LayawayRefundPayments
                {
                    Id = dto.Id,
                    tbirp_Layaway_RefundId = dto.tbirp_Layaway_RefundId,
                    tbirp_RefundMethodId = dto.tbirp_RefundMethodId,
                    tbirp_RefundAmt = dto.tbirp_RefundAmt,
                    tbirp_Date = dto.tbirp_Date
                };
                list.Add(payment);
            }
            return list;
        }

        private void UpdateLayawayRefundMaster(LayawayRefundMaster lm, LayawayRefundMasterDto dto)
        {
            lm.Id = dto.Id;
            lm.tbirm_LayawayRefundIdRad = dto.tbirm_LayawayRefundIdRad;
            lm.tbirm_LayawayRefundDate = dto.tbirm_LayawayRefundDate;
            lm.tbirm_RefundType = dto.tbirm_RefundType;
            lm.tbirm_SubTotal = dto.tbirm_SubTotal;
            lm.tbirm_SaleTax = dto.tbirm_SaleTax;
            lm.tbirm_Labour = dto.tbirm_Labour;
            lm.tbirm_DisPer = dto.tbirm_DisPer;
            lm.tbirm_DisAmt = dto.tbirm_DisAmt;
            lm.tbirm_Total = dto.tbirm_Total;
            lm.tbirm_RefundAmt = dto.tbirm_RefundAmt;
            lm.tbirm_AdjAmt = dto.tbirm_AdjAmt;
            lm.tbirm_Note = dto.tbirm_Note;
            lm.tbirm_Delinfo = dto.tbirm_Delinfo;
            lm.tbirm_Item_Delete_after_Layaway_Refund_Create = dto.tbirm_Item_Delete_after_Layaway_Refund_Create;
            lm.UserName = dto.UserName;
            lm.SetDate = dto.SetDate;
            lm.Layaway_tbim_InvoiceId = dto.Layaway_tbim_InvoiceId;
            lm.Layaway_tbim_InvoiceIdRad = dto.Layaway_tbim_InvoiceIdRad;
            lm.Layaway_tbim_Phone = dto.Layaway_tbim_Phone;
            lm.Layaway_tbim_InvDate = dto.Layaway_tbim_InvDate;
            lm.Layaway_tbim_Name = dto.Layaway_tbim_Name;
            lm.Layaway_tbim_TaxId = dto.Layaway_tbim_TaxId;
            lm.Layaway_tbim_VehicleMake = dto.Layaway_tbim_VehicleMake;
            lm.Layaway_tbim_Model = dto.Layaway_tbim_Model;
            lm.Layaway_tbim_Year = dto.Layaway_tbim_Year;
            lm.Layaway_tbim_Odometer = dto.Layaway_tbim_Odometer;
            lm.Layaway_tbim_TreadDepth = dto.Layaway_tbim_TreadDepth;
            lm.Layaway_tbim_License = dto.Layaway_tbim_License;
            lm.Layaway_tbim_SubTotal = dto.Layaway_tbim_SubTotal;
            lm.Layaway_tbim_SaleTax = dto.Layaway_tbim_SaleTax;
            lm.Layaway_tbim_Labour = dto.Layaway_tbim_Labour;
            lm.Layaway_tbim_DisPer = dto.Layaway_tbim_DisPer;
            lm.Layaway_tbim_DisAmt = dto.Layaway_tbim_DisAmt;
            lm.Layaway_tbim_Total = dto.Layaway_tbim_Total;
            lm.Layaway_tbim_PaidAmt = dto.Layaway_tbim_PaidAmt;
            lm.Layaway_tbim_AdjAmt = dto.Layaway_tbim_AdjAmt;
            lm.Layaway_tbim_AdjTotal = dto.Layaway_tbim_AdjTotal;
            lm.Layaway_tbim_PayInfo = dto.Layaway_tbim_PayInfo;
            lm.Layaway_tbim_Note = dto.Layaway_tbim_Note;
            lm.Layaway_tbim_Delinfo = dto.Layaway_tbim_Delinfo;
            lm.Layaway_tbim_CompanyName = dto.Layaway_tbim_CompanyName;
            lm.Layaway_tbim_CompanyAddress = dto.Layaway_tbim_CompanyAddress;
            lm.Layaway_tbim_Item_Delete_after_Layaway_Create = dto.Layaway_tbim_Item_Delete_after_Layaway_Create;
            lm.Layaway_tbim_Left_Front = dto.Layaway_tbim_Left_Front;
            lm.Layaway_tbim_Right_Front = dto.Layaway_tbim_Right_Front;
            lm.Layaway_tbim_Left_Rear = dto.Layaway_tbim_Left_Rear;
            lm.Layaway_tbim_Right_Rear = dto.Layaway_tbim_Right_Rear;
            lm.Layaway_tbim_EmailAddress = dto.Layaway_tbim_EmailAddress;
            lm.Layaway_tbim_IDNo = dto.Layaway_tbim_IDNo;
        }

        private void ProcessLayawayRefundDetailsUpsert(LayawayRefundMaster refundMaster, List<LayawayRefundDetailsDto> incomingDetails)
        {
            var incomingDetailIds = incomingDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = refundMaster.LayawayRefundDetails.Where(d => !incomingDetailIds.Contains(d.Id)).ToList();
            _context.LayawayRefundDetails.RemoveRange(detailsToRemove);

            Distributors distributor = new Distributors();

            foreach (var detailDto in incomingDetails)
            {
                if (detailDto.Id == 0)
                {
                    var itemMaster = detailDto.tbird_ItemId.HasValue ? _context.ItemMasters.Find(detailDto.tbird_ItemId.Value) : null;
                    var dept = itemMaster != null ? _context.Departments.Find(itemMaster.tbim_ItemCategoryId) : null;

                    if (itemMaster != null && itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    refundMaster.LayawayRefundDetails.Add(new LayawayRefundDetails
                    {
                        tbird_Layaway_RefundId = refundMaster.Id,
                        tbird_ItemId = detailDto.tbird_ItemId,
                        tbird_ItemCategory = itemMaster == null ? null : (int?)itemMaster.tbim_ItemCategoryId,
                        tbird_DepartmentName = dept?.Tbid_DepartmentName,
                        tbird_Size = detailDto.tbird_Size,
                        tbird_Brand = detailDto.tbird_Brand,
                        tbird_Series = detailDto.tbird_Series,
                        tbird_Bolt = detailDto.tbird_Bolt,
                        tbird_HoleS = detailDto.tbird_HoleS,
                        tbird_Zone = detailDto.tbird_Zone,
                        tbird_DistributorId = detailDto.tbird_DistributorId,
                        tbird_DistributorName = distributor?.Name,
                        tbird_Qty = detailDto.tbird_Qty,
                        tbird_Layaway_Qty = detailDto.tbird_Layaway_Qty,
                        tbird_Layaway_Qty_LineTotal = detailDto.tbird_Layaway_Qty_LineTotal,
                        tbird_Layaway_Qty_TaxAmt = detailDto.tbird_Layaway_Qty_TaxAmt,
                        tbird_Taxable = detailDto.tbird_Taxable,
                        tbird_UnitPrice = detailDto.tbird_UnitPrice,
                        tbird_LineTotal = detailDto.tbird_LineTotal,
                        tbird_TaxRate = detailDto.tbird_TaxRate,
                        tbird_TaxAmt = detailDto.tbird_TaxAmt
                    });
                }
                else
                {
                    var existingDetail = refundMaster.LayawayRefundDetails.FirstOrDefault(d => d.Id == detailDto.Id);
                    var itemMaster = detailDto.tbird_ItemId.HasValue ? _context.ItemMasters.Find(detailDto.tbird_ItemId.Value) : null;
                    var dept = itemMaster != null ? _context.Departments.Find(itemMaster.tbim_ItemCategoryId) : null;

                    if (itemMaster != null && itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    if (existingDetail != null)
                    {
                        existingDetail.tbird_Layaway_RefundId = refundMaster.Id;
                        existingDetail.tbird_ItemId = detailDto.tbird_ItemId;
                        existingDetail.tbird_ItemCategory = itemMaster == null ? null : (int?)itemMaster.tbim_ItemCategoryId;
                        existingDetail.tbird_DepartmentName = dept?.Tbid_DepartmentName;
                        existingDetail.tbird_Size = detailDto.tbird_Size;
                        existingDetail.tbird_Brand = detailDto.tbird_Brand;
                        existingDetail.tbird_Series = detailDto.tbird_Series;
                        existingDetail.tbird_Bolt = detailDto.tbird_Bolt;
                        existingDetail.tbird_HoleS = detailDto.tbird_HoleS;
                        existingDetail.tbird_Zone = detailDto.tbird_Zone;
                        existingDetail.tbird_DistributorId = detailDto.tbird_DistributorId;
                        existingDetail.tbird_DistributorName = distributor?.Name;
                        existingDetail.tbird_Qty = detailDto.tbird_Qty;
                        existingDetail.tbird_Layaway_Qty = detailDto.tbird_Layaway_Qty;
                        existingDetail.tbird_Layaway_Qty_LineTotal = detailDto.tbird_Layaway_Qty_LineTotal;
                        existingDetail.tbird_Layaway_Qty_TaxAmt = detailDto.tbird_Layaway_Qty_TaxAmt;
                        existingDetail.tbird_Taxable = detailDto.tbird_Taxable;
                        existingDetail.tbird_UnitPrice = detailDto.tbird_UnitPrice;
                        existingDetail.tbird_LineTotal = detailDto.tbird_LineTotal;
                        existingDetail.tbird_TaxRate = detailDto.tbird_TaxRate;
                        existingDetail.tbird_TaxAmt = detailDto.tbird_TaxAmt;
                    }
                }
            }
        }

        private void ProcessLayawayRefundPaymentsUpsert(LayawayRefundMaster refundMaster, List<LayawayRefundPaymentsDto> incomingPayments)
        {
            var incomingPaymentIds = incomingPayments.Where(p => p.Id > 0).Select(p => p.Id).ToList();
            var paymentsToRemove = refundMaster.LayawayRefundPayments.Where(p => !incomingPaymentIds.Contains(p.Id)).ToList();
            _context.LayawayRefundPayments.RemoveRange(paymentsToRemove);

            foreach (var payDto in incomingPayments)
            {
                if (payDto.Id == 0)
                {
                    refundMaster.LayawayRefundPayments.Add(new LayawayRefundPayments
                    {
                        tbirp_Layaway_RefundId = refundMaster.Id,
                        tbirp_RefundMethodId = payDto.tbirp_RefundMethodId,
                        tbirp_RefundAmt = payDto.tbirp_RefundAmt,
                        tbirp_Date = DateTime.UtcNow
                    });
                }
                else
                {
                    var existingPayment = refundMaster.LayawayRefundPayments.FirstOrDefault(p => p.Id == payDto.Id);
                    if (existingPayment != null)
                    {
                        existingPayment.tbirp_Layaway_RefundId = refundMaster.Id;
                        existingPayment.tbirp_RefundMethodId = payDto.tbirp_RefundMethodId;
                        existingPayment.tbirp_RefundAmt = payDto.tbirp_RefundAmt;
                        existingPayment.tbirp_Date = payDto.tbirp_Date;
                    }
                }
            }
        }
    }
}