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
    public class LayawayMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LayawayMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/LayawayMaster
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreateLayawayDto>>> GetLayawayMasters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            var masters = await _context.LayawayMasters
                .OrderByDescending(m => m.tbim_InvDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<CreateLayawayDto>();
            foreach (var lm in masters)
            {
                dtos.Add(MapToCreateLayawayDto(lm));
            }

            int totalRecords = await _context.LayawayMasters.CountAsync();
            Response.Headers.Add("X-Total-Count", totalRecords.ToString());

            return Ok(dtos);
        }

        // GET: api/LayawayMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreateLayawayDto>> GetLayawayMaster(long id)
        {
            var layawayMaster = await _context.LayawayMasters
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (layawayMaster == null) return NotFound();
            return Ok(MapToCreateLayawayDto(layawayMaster));
        }

        // POST: api/LayawayMaster/EditLayaway
        [HttpPost("EditLayaway")]
        public async Task<IActionResult> EditLayaway(long id, CreateLayawayDto createLayawayDto)
        {
            if (createLayawayDto == null) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var layawayMaster = await _context.LayawayMasters
                    .Include(m => m.LayawayDetails)
                    .Include(m => m.LayawayPayments)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (layawayMaster == null) return NotFound();

                UpdateLayawayMaster(layawayMaster, createLayawayDto.LayawayMasterDto ?? new LayawayMasterDto());
                _context.Entry(layawayMaster).State = EntityState.Modified;

                ProcessLayawayDetailsUpsert(layawayMaster, createLayawayDto.LayawayDetailsDto ?? new List<LayawayDetailsDto>());
                ProcessLayawayPaymentsUpsert(layawayMaster, createLayawayDto.LayawayPaymentsDto ?? new List<LayawayPaymentsDto>());

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "Layaway adjusted successfully", LayawayId = id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the layaway.");
            }
        }

        // POST: api/LayawayMaster/CreateLayaway
        // Accepts a master, a list of details and a list of payments.
        // Inserts master first, then inserts details and payments with FK set to master.Id
        [HttpPost("CreateLayaway")]
        public async Task<ActionResult<CreateLayawayDto>> CreateLayaway(CreateLayawayDto createLayawayDto)
        {
            if (createLayawayDto == null) return BadRequest();

            var layawayMaster = MapToLayawayMaster(createLayawayDto.LayawayMasterDto ?? new LayawayMasterDto());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.LayawayMasters.Add(layawayMaster);
                await _context.SaveChangesAsync();

                var details = MapToLayawayDetails(createLayawayDto.LayawayDetailsDto ?? new List<LayawayDetailsDto>());
                foreach (var d in details)
                {
                    d.tbid_InvoiceId = layawayMaster.Id;
                    d.Id = 0;
                    d.tbid_Invoice = null;

                    if (d.tbid_ItemId.HasValue)
                    {
                        var itemMaster = await _context.ItemMasters.FindAsync(d.tbid_ItemId.Value);
                        if (itemMaster != null)
                        {
                            try
                            {
                                d.tbid_ItemCategory = (long?)itemMaster.tbim_ItemCategoryId;
                            }
                            catch
                            {
                                d.tbid_ItemCategory = null;
                            }

                            var dept = await _context.Departments.FindAsync(itemMaster.tbim_ItemCategoryId);
                            d.tbid_DepartmentName = dept?.Tbid_DepartmentName;

                            d.tbid_Size = itemMaster.tbim_Size;
                            d.tbid_Brand = itemMaster.tbim_Brand;
                            d.tbid_Series = itemMaster.tbim_Series;
                            d.tbid_Bolt = itemMaster.tbim_Bolt;
                            d.tbid_HoleS = itemMaster.tbim_HoleS;
                            d.tbid_Zone = itemMaster.tbim_Zone;
                            d.tbid_DistributorId = itemMaster.tbim_DistributorId;

                            if (itemMaster.tbim_DistributorId.HasValue)
                            {
                                var distributor = await _context.Distributors.FindAsync(itemMaster.tbim_DistributorId.Value);
                                d.tbid_DistributorName = distributor?.Name;
                            }
                        }
                    }
                }

                if (details.Count > 0)
                {
                    _context.LayawayDetails.AddRange(details);
                }

                var payments = MapToLayawayPayments(createLayawayDto.LayawayPaymentsDto ?? new List<LayawayPaymentsDto>());
                foreach (var p in payments)
                {
                    p.tbip_InvoiceId = layawayMaster.Id;
                    p.Id = 0;
                    p.tbip_Invoice = null;
                }

                if (payments.Count > 0)
                {
                    _context.LayawayPayments.AddRange(payments);
                }

                await _context.SaveChangesAsync();

                var created = await _context.LayawayMasters
                    .Include(m => m.LayawayDetails)
                    .Include(m => m.LayawayPayments)
                    .FirstOrDefaultAsync(m => m.Id == layawayMaster.Id);

                await transaction.CommitAsync();
                return await GetLayawayMaster(layawayMaster.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Critical database transaction failure while creating the layaway.");
            }
        }

        // DELETE: api/LayawayMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLayawayMaster(long id)
        {
            var layawayMaster = await _context.LayawayMasters.FindAsync(id);
            if (layawayMaster == null) return NotFound();

            _context.LayawayMasters.Remove(layawayMaster);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool LayawayMasterExists(long id)
        {
            return _context.LayawayMasters.Any(e => e.Id == id);
        }

        // Mapping helpers (parallel to Invoice controller)
        private CreateLayawayDto MapToCreateLayawayDto(LayawayMaster lm)
        {
            return new CreateLayawayDto
            {
                LayawayMasterDto = MapToLayawayMasterDto(lm),
                LayawayDetailsDto = MapToLayawayDetailsDto(lm.Id),
                LayawayPaymentsDto = MapToLayawayPaymentsDto(lm.Id)
            };
        }

        private LayawayMasterDto MapToLayawayMasterDto(LayawayMaster lm)
        {
            return new LayawayMasterDto
            {
                Id = lm.Id,
                tbim_InvoiceIdRad = lm.tbim_InvoiceIdRad,
                tbim_Phone = lm.tbim_Phone,
                tbim_InvDate = lm.tbim_InvDate,
                tbim_Name = lm.tbim_Name,
                tbim_TaxId = lm.tbim_TaxId,
                tbim_VehicleMake = lm.tbim_VehicleMake,
                tbim_Model = lm.tbim_Model,
                tbim_Year = lm.tbim_Year,
                tbim_Odometer = lm.tbim_Odometer,
                tbim_TreadDepth = lm.tbim_TreadDepth,
                tbim_License = lm.tbim_License,
                tbim_SubTotal = lm.tbim_SubTotal,
                tbim_SaleTax = lm.tbim_SaleTax,
                tbim_Labour = lm.tbim_Labour,
                tbim_DisPer = lm.tbim_DisPer,
                tbim_DisAmt = lm.tbim_DisAmt,
                tbim_Total = lm.tbim_Total,
                tbim_PaidAmt = lm.tbim_PaidAmt,
                tbim_AdjAmt = lm.tbim_AdjAmt,
                tbim_AdjTotal = lm.tbim_AdjTotal,
                tbim_PayInfo = lm.tbim_PayInfo,
                tbim_Note = lm.tbim_Note,
                tbim_Delinfo = lm.tbim_Delinfo,
                tbim_CompanyName = lm.tbim_CompanyName,
                tbim_CompanyAddress = lm.tbim_CompanyAddress,
                tbim_Item_Delete_after_Layaway_Create = lm.tbim_Item_Delete_after_Layaway_Create,
                UserName = lm.UserName,
                SetDate = lm.SetDate,
                tbim_Left_Front = lm.tbim_Left_Front,
                tbim_Right_Front = lm.tbim_Right_Front,
                tbim_Left_Rear = lm.tbim_Left_Rear,
                tbim_Right_Rear = lm.tbim_Right_Rear,
                tbim_EmailAddress = lm.tbim_EmailAddress,
                tbim_IDNo = lm.tbim_IDNo,
                tbim_RefundType = lm.tbim_RefundType,
                tbim_LocationId = lm.tbim_LocationId,
                LocationName = lm.tbim_Location != null ? lm.tbim_Location.tbld_LocationName : string.Empty,
                TaxCompanyName = lm.tbim_Tax != null ? lm.tbim_Tax.tbti_ComName : string.Empty
            };
        }

        private List<LayawayDetailsDto> MapToLayawayDetailsDto(long Id)
        {
            var list = _context.LayawayDetails
                .Where(d => d.tbid_InvoiceId == Id)
                .Select(d => new LayawayDetailsDto
                {
                    Id = d.Id,
                    tbid_InvoiceId = d.tbid_InvoiceId,
                    tbid_ItemId = d.tbid_ItemId,
                    tbid_ItemCategory = d.tbid_ItemCategory,
                    tbid_DepartmentName = d.tbid_DepartmentName,
                    tbid_Size = d.tbid_Size,
                    tbid_Brand = d.tbid_Brand,
                    tbid_Series = d.tbid_Series,
                    tbid_Bolt = d.tbid_Bolt,
                    tbid_HoleS = d.tbid_HoleS,
                    tbid_Zone = d.tbid_Zone,
                    tbid_DistributorId = d.tbid_DistributorId,
                    tbid_DistributorName = d.tbid_DistributorName,
                    tbid_Qty = d.tbid_Qty,
                    tbid_Taxable = d.tbid_Taxable,
                    tbid_UnitPrice = d.tbid_UnitPrice,
                    tbid_LineTotal = d.tbid_LineTotal,
                    tbid_TaxAmt = d.tbid_TaxAmt,
                    ItemDepartmentName = d.tbid_Item != null && d.tbid_Item.tbim_ItemCategory != null
                        ? d.tbid_Item.tbim_ItemCategory.Tbid_DepartmentName : string.Empty,
                    ItemDistributorName = d.tbid_Item != null && d.tbid_Item.tbim_Distributor != null
                        ? d.tbid_Item.tbim_Distributor.Name : string.Empty,
                    ItemLocationName = d.tbid_Item != null && d.tbid_Item.tbim_Location != null
                        ? d.tbid_Item.tbim_Location.tbld_LocationName : string.Empty,
                    ItemDisplay = d.tbid_Item != null
                        ? (d.tbid_Item.tbim_Brand + " " + d.tbid_Item.tbim_Size).Trim() : string.Empty
                })
                .ToList();

            return list;
        }

        private List<LayawayPaymentsDto> MapToLayawayPaymentsDto(long Id)
        {
            var list = (from p in _context.LayawayPayments
                        join pay in _context.PaymentNames
                            on p.tbip_PaymentId equals pay.Id into gj
                        from pay in gj.DefaultIfEmpty()
                        where p.tbip_InvoiceId == Id
                        select new LayawayPaymentsDto
                        {
                            Id = p.Id,
                            tbip_InvoiceId = p.tbip_InvoiceId,
                            tbip_PaymentId = p.tbip_PaymentId,
                            tbip_PayAmt = p.tbip_PayAmt,
                            tbip_Date = p.tbip_Date,
                            tbip_PaymentType = p.tbip_PaymentType,
                            PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                        })
                        .ToList();

            return list;
        }

        private LayawayMaster MapToLayawayMaster(LayawayMasterDto dto)
        {
            return new LayawayMaster
            {
                Id = dto.Id,
                tbim_InvoiceIdRad = dto.tbim_InvoiceIdRad,
                tbim_Phone = dto.tbim_Phone,
                tbim_InvDate = dto.tbim_InvDate,
                tbim_Name = dto.tbim_Name,
                tbim_TaxId = dto.tbim_TaxId,
                tbim_VehicleMake = dto.tbim_VehicleMake,
                tbim_Model = dto.tbim_Model,
                tbim_Year = dto.tbim_Year,
                tbim_Odometer = dto.tbim_Odometer,
                tbim_TreadDepth = dto.tbim_TreadDepth,
                tbim_License = dto.tbim_License,
                tbim_SubTotal = dto.tbim_SubTotal,
                tbim_SaleTax = dto.tbim_SaleTax,
                tbim_Labour = dto.tbim_Labour,
                tbim_DisPer = dto.tbim_DisPer,
                tbim_DisAmt = dto.tbim_DisAmt,
                tbim_Total = dto.tbim_Total,
                tbim_PaidAmt = dto.tbim_PaidAmt,
                tbim_AdjAmt = dto.tbim_AdjAmt,
                tbim_AdjTotal = dto.tbim_AdjTotal,
                tbim_PayInfo = dto.tbim_PayInfo,
                tbim_Note = dto.tbim_Note,
                tbim_Delinfo = dto.tbim_Delinfo,
                tbim_CompanyName = dto.tbim_CompanyName,
                tbim_CompanyAddress = dto.tbim_CompanyAddress,
                tbim_Item_Delete_after_Layaway_Create = dto.tbim_Item_Delete_after_Layaway_Create,
                UserName = dto.UserName,
                SetDate = dto.SetDate,
                tbim_Left_Front = dto.tbim_Left_Front,
                tbim_Right_Front = dto.tbim_Right_Front,
                tbim_Left_Rear = dto.tbim_Left_Rear,
                tbim_Right_Rear = dto.tbim_Right_Rear,
                tbim_EmailAddress = dto.tbim_EmailAddress,
                tbim_IDNo = dto.tbim_IDNo,
                tbim_RefundType = dto.tbim_RefundType,
                tbim_LocationId = dto.tbim_LocationId,

            };
        }

        private List<LayawayDetails> MapToLayawayDetails(List<LayawayDetailsDto> dtos)
        {
            var list = new List<LayawayDetails>();
            foreach (var dto in dtos)
            {
                var detail = new LayawayDetails
                {
                    Id = dto.Id,
                    tbid_InvoiceId = dto.tbid_InvoiceId,
                    tbid_ItemId = dto.tbid_ItemId,
                    tbid_ItemCategory = dto.tbid_ItemCategory,
                    tbid_DepartmentName = dto.tbid_DepartmentName,
                    tbid_Size = dto.tbid_Size,
                    tbid_Brand = dto.tbid_Brand,
                    tbid_Series = dto.tbid_Series,
                    tbid_Bolt = dto.tbid_Bolt,
                    tbid_HoleS = dto.tbid_HoleS,
                    tbid_Zone = dto.tbid_Zone,
                    tbid_DistributorId = dto.tbid_DistributorId,
                    tbid_DistributorName = dto.tbid_DistributorName,
                    tbid_Qty = dto.tbid_Qty,
                    tbid_Taxable = dto.tbid_Taxable,
                    tbid_UnitPrice = dto.tbid_UnitPrice,
                    tbid_LineTotal = dto.tbid_LineTotal,
                    tbid_TaxAmt = dto.tbid_TaxAmt
                };
                list.Add(detail);
            }
            return list;
        }

        private List<LayawayPayments> MapToLayawayPayments(List<LayawayPaymentsDto> dtos)
        {
            var list = new List<LayawayPayments>();
            foreach (var dto in dtos)
            {
                var payment = new LayawayPayments
                {
                    Id = dto.Id,
                    tbip_InvoiceId = dto.tbip_InvoiceId,
                    tbip_PaymentId = dto.tbip_PaymentId,
                    tbip_PayAmt = dto.tbip_PayAmt,
                    tbip_Date = dto.tbip_Date,
                    tbip_PaymentType = dto.tbip_PaymentType
                };
                list.Add(payment);
            }
            return list;
        }

        private void UpdateLayawayMaster(LayawayMaster lm, LayawayMasterDto dto)
        {
            lm.Id = dto.Id;
            lm.tbim_InvoiceIdRad = dto.tbim_InvoiceIdRad;
            lm.tbim_Phone = dto.tbim_Phone;
            lm.tbim_InvDate = dto.tbim_InvDate;
            lm.tbim_Name = dto.tbim_Name;
            lm.tbim_TaxId = dto.tbim_TaxId;
            lm.tbim_VehicleMake = dto.tbim_VehicleMake;
            lm.tbim_Model = dto.tbim_Model;
            lm.tbim_Year = dto.tbim_Year;
            lm.tbim_Odometer = dto.tbim_Odometer;
            lm.tbim_TreadDepth = dto.tbim_TreadDepth;
            lm.tbim_License = dto.tbim_License;
            lm.tbim_SubTotal = dto.tbim_SubTotal;
            lm.tbim_SaleTax = dto.tbim_SaleTax;
            lm.tbim_Labour = dto.tbim_Labour;
            lm.tbim_DisPer = dto.tbim_DisPer;
            lm.tbim_DisAmt = dto.tbim_DisAmt;
            lm.tbim_Total = dto.tbim_Total;
            lm.tbim_PaidAmt = dto.tbim_PaidAmt;
            lm.tbim_AdjAmt = dto.tbim_AdjAmt;
            lm.tbim_AdjTotal = dto.tbim_AdjTotal;
            lm.tbim_PayInfo = dto.tbim_PayInfo;
            lm.tbim_Note = dto.tbim_Note;
            lm.tbim_Delinfo = dto.tbim_Delinfo;
            lm.tbim_CompanyName = dto.tbim_CompanyName;
            lm.tbim_CompanyAddress = dto.tbim_CompanyAddress;
            lm.tbim_Item_Delete_after_Layaway_Create = dto.tbim_Item_Delete_after_Layaway_Create;
            lm.UserName = dto.UserName;
            lm.SetDate = dto.SetDate;
            lm.tbim_Left_Front = dto.tbim_Left_Front;
            lm.tbim_Right_Front = dto.tbim_Right_Front;
            lm.tbim_Left_Rear = dto.tbim_Left_Rear;
            lm.tbim_Right_Rear = dto.tbim_Right_Rear;
            lm.tbim_EmailAddress = dto.tbim_EmailAddress;
            lm.tbim_IDNo = dto.tbim_IDNo;
            lm.tbim_RefundType = dto.tbim_RefundType;
            lm.tbim_LocationId = dto.tbim_LocationId;

        }

        private void ProcessLayawayDetailsUpsert(LayawayMaster layawayMaster, List<LayawayDetailsDto> incomingDetails)
        {
            var incomingDetailIds = incomingDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = layawayMaster.LayawayDetails.Where(d => !incomingDetailIds.Contains(d.Id)).ToList();
            _context.LayawayDetails.RemoveRange(detailsToRemove);

            Distributors distributor = new Distributors();

            foreach (var detailDto in incomingDetails)
            {
                if (detailDto.Id == 0)
                {
                    var itemMaster = detailDto.tbid_ItemId.HasValue ? _context.ItemMasters.Find(detailDto.tbid_ItemId.Value) : null;
                    var dept = itemMaster != null ? _context.Departments.Find(itemMaster.tbim_ItemCategoryId) : null;

                    if (itemMaster != null && itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    layawayMaster.LayawayDetails.Add(new LayawayDetails
                    {
                        tbid_InvoiceId = layawayMaster.Id,
                        tbid_ItemId = detailDto.tbid_ItemId,
                        tbid_ItemCategory = itemMaster == null ? null : (long?)itemMaster.tbim_ItemCategoryId,
                        tbid_DepartmentName = dept?.Tbid_DepartmentName,
                        tbid_Size = detailDto.tbid_Size,
                        tbid_Brand = detailDto.tbid_Brand,
                        tbid_Series = detailDto.tbid_Series,
                        tbid_Bolt = detailDto.tbid_Bolt,
                        tbid_HoleS = detailDto.tbid_HoleS,
                        tbid_Zone = detailDto.tbid_Zone,
                        tbid_DistributorId = detailDto.tbid_DistributorId,
                        tbid_DistributorName = distributor?.Name,
                        tbid_Qty = detailDto.tbid_Qty,
                        tbid_Taxable = detailDto.tbid_Taxable,
                        tbid_UnitPrice = detailDto.tbid_UnitPrice,
                        tbid_LineTotal = detailDto.tbid_LineTotal,
                        tbid_TaxAmt = detailDto.tbid_TaxAmt
                    });
                }
                else
                {
                    var existingDetail = layawayMaster.LayawayDetails.FirstOrDefault(d => d.Id == detailDto.Id);
                    var itemMaster = detailDto.tbid_ItemId.HasValue ? _context.ItemMasters.Find(detailDto.tbid_ItemId.Value) : null;
                    var dept = itemMaster != null ? _context.Departments.Find(itemMaster.tbim_ItemCategoryId) : null;

                    if (itemMaster != null && itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    if (existingDetail != null)
                    {
                        existingDetail.tbid_InvoiceId = layawayMaster.Id;
                        existingDetail.tbid_ItemId = detailDto.tbid_ItemId;
                        existingDetail.tbid_ItemCategory = itemMaster == null ? null : (long?)itemMaster.tbim_ItemCategoryId;
                        existingDetail.tbid_DepartmentName = dept?.Tbid_DepartmentName;
                        existingDetail.tbid_Size = detailDto.tbid_Size;
                        existingDetail.tbid_Brand = detailDto.tbid_Brand;
                        existingDetail.tbid_Series = detailDto.tbid_Series;
                        existingDetail.tbid_Bolt = detailDto.tbid_Bolt;
                        existingDetail.tbid_HoleS = detailDto.tbid_HoleS;
                        existingDetail.tbid_Zone = detailDto.tbid_Zone;
                        existingDetail.tbid_DistributorId = detailDto.tbid_DistributorId;
                        existingDetail.tbid_DistributorName = distributor?.Name;
                        existingDetail.tbid_Qty = detailDto.tbid_Qty;
                        existingDetail.tbid_Taxable = detailDto.tbid_Taxable;
                        existingDetail.tbid_UnitPrice = detailDto.tbid_UnitPrice;
                        existingDetail.tbid_LineTotal = detailDto.tbid_LineTotal;
                        existingDetail.tbid_TaxAmt = detailDto.tbid_TaxAmt;
                    }
                }
            }
        }

        private void ProcessLayawayPaymentsUpsert(LayawayMaster layawayMaster, List<LayawayPaymentsDto> incomingPayments)
        {
            var incomingPaymentIds = incomingPayments.Where(p => p.Id > 0).Select(p => p.Id).ToList();
            var paymentsToRemove = layawayMaster.LayawayPayments.Where(p => !incomingPaymentIds.Contains(p.Id)).ToList();
            _context.LayawayPayments.RemoveRange(paymentsToRemove);

            foreach (var payDto in incomingPayments)
            {
                if (payDto.Id == 0)
                {
                    layawayMaster.LayawayPayments.Add(new LayawayPayments
                    {
                        tbip_InvoiceId = layawayMaster.Id,
                        tbip_PaymentId = payDto.tbip_PaymentId,
                        tbip_PayAmt = payDto.tbip_PayAmt,
                        tbip_Date = DateTime.UtcNow,
                        tbip_PaymentType = payDto.tbip_PaymentType
                    });
                }
                else
                {
                    var existingPayment = layawayMaster.LayawayPayments.FirstOrDefault(p => p.Id == payDto.Id);
                    if (existingPayment != null)
                    {
                        existingPayment.tbip_InvoiceId = layawayMaster.Id;
                        existingPayment.tbip_PaymentId = payDto.tbip_PaymentId;
                        existingPayment.tbip_PayAmt = payDto.tbip_PayAmt;
                        existingPayment.tbip_Date = payDto.tbip_Date;
                        existingPayment.tbip_PaymentType = payDto.tbip_PaymentType;
                    }
                }
            }
        }
    }
}