using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TireInventory.Data;
using TireInventory.Helpers;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceRefundMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceRefundMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceRefundMaster
        [HttpGet]
        public async Task<ActionResult<PagedInvoiceRefundResponseDto>> GetInvoiceRefundMasters(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] long? refundTransactionId = null,
            [FromQuery] long? invoiceTransactionId = null,
            [FromQuery] string? customerName = null,
            [FromQuery] string? phoneNo = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
            )
        {
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            // Start with a base, un-executed query
            var query = _context.InvoiceRefundMasters.AsNoTracking();

            // --- Dynamic Filtering Blocks ---
            if (startDate.HasValue)
            {
                query = query.Where(o => o.tbirm_InvRefundDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var inclusiveEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.tbirm_InvRefundDate <= inclusiveEndDate);
            }

            if (refundTransactionId.HasValue)
            {
                query = query.Where(o => o.tbirm_InvoiceRefundIdRad == refundTransactionId.Value);
            }

            if (invoiceTransactionId.HasValue)
            {
                query = query.Where(o => o.tbirm_InvoiceRefundIdRad == invoiceTransactionId.Value);
            }

            //if (!string.IsNullOrWhiteSpace(customerName))
            //{
            //    query = query.Where(o => o.OriginalInvoiceName.Contains(customerName.Trim()));
            //}

            //if (!string.IsNullOrWhiteSpace(phoneNo))
            //{
            //    var cleanPhone = phoneNo.Trim();
            //    query = query.Where(o => o.tbim_Phone.Contains(cleanPhone));
            //}

            // --- CRUCIAL FIX: Calculate total count based on the FILTERED query parameters ---
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);


            var masters = await query
                .Include(m=>m.InvoiceRefundDetails)
                .Include(m=>m.InvoiceRefundPayments)
                .Include(m=>m.tbirm_Invoice)
                .OrderByDescending(m => m.tbirm_InvRefundDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<CreateInvoiceRefundDto>();
            foreach (var m in masters)
            {
                dtos.Add(MapToCreateInvoiceRefundDto(m));
            }

            // 3. Build and return the structured response object directly matching your layout
            var response = new PagedInvoiceRefundResponseDto
            {
                Items = dtos,
                TotalCount = totalRecords,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return Ok(response);

        }

        // GET: api/InvoiceRefundMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreateInvoiceRefundDto>> GetInvoiceRefundMaster(long id)
        {
            var refundMaster = await _context.InvoiceRefundMasters
                .Where(m => m.Id == id)
                .Include(m => m.InvoiceRefundDetails)
                .Include(m => m.InvoiceRefundPayments)
                .Include(m => m.tbirm_Invoice)
                .FirstOrDefaultAsync();

            if (refundMaster == null) return NotFound();
            return Ok(MapToCreateInvoiceRefundDto(refundMaster));
        }

        // POST: api/InvoiceRefundMaster/EditRefund
        [HttpPost("EditRefund")]
        public async Task<IActionResult> EditRefund(long id, CreateInvoiceRefundDto createDto)
        {
            if (createDto == null) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var refundMaster = await _context.InvoiceRefundMasters
                    .Include(m => m.InvoiceRefundDetails)
                    .Include(m => m.InvoiceRefundPayments)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (refundMaster == null) return NotFound();

                UpdateInvoiceRefundMaster(refundMaster, createDto.invoiceRefundMasterDto ?? new InvoiceRefundMasterDto());
                _context.Entry(refundMaster).State = EntityState.Modified;

                ProcessInvoiceRefundDetailsUpsert(refundMaster, createDto.invoiceRefundDetailsDto ?? new List<InvoiceRefundDetailsDto>());
                ProcessInvoiceRefundPaymentsUpsert(refundMaster, createDto.invoiceRefundPaymentsDto ?? new List<InvoiceRefundPaymentsDto>());

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "Refund adjusted successfully", RefundId = id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the refund.");
            }
        }

        // POST: api/InvoiceRefundMaster/CreateRefund
        // Mirrors invoice flow: insert refund master first, then insert refund details and refund payments with FK set to refundMaster.Id
        [HttpPost("CreateRefund")]
        public async Task<ActionResult<CreateInvoiceRefundDto>> CreateRefund(CreateInvoiceRefundDto createDto)
        {
            if (createDto == null) return BadRequest();

            var refundMaster = MapToInvoiceRefundMaster(createDto.invoiceRefundMasterDto ?? new InvoiceRefundMasterDto());
            refundMaster.tbirm_InvoiceRefundIdRad = CommonFunctions.GenerateTransactionID();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.InvoiceRefundMasters.Add(refundMaster);
                await _context.SaveChangesAsync();

                // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors
                var details = MapToInvoiceRefundDetails(createDto.invoiceRefundDetailsDto ?? new List<InvoiceRefundDetailsDto>());
                foreach (var d in details)
                {
                    d.tbird_InvoiceRefundId = refundMaster.Id;
                    d.Id = 0;
                    d.tbird_InvoiceRefund = null;

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
                    _context.InvoiceRefundDetails.AddRange(details);
                }

                // PAYMENTS: set FK to created refund master Id and insert
                var payments = MapToInvoiceRefundPayments(createDto.invoiceRefundPaymentsDto ?? new List<InvoiceRefundPaymentsDto>());
                foreach (var p in payments)
                {
                    p.tbirp_InvoiceRefundId = refundMaster.Id;
                    p.Id = 0;
                    p.tbirp_InvoiceRefund = null;
                }

                if (payments.Count > 0)
                {
                    _context.InvoiceRefundPayments.AddRange(payments);
                }

                await _context.SaveChangesAsync();

                var created = await _context.InvoiceRefundMasters
                    .Include(m => m.InvoiceRefundDetails)
                    .Include(m => m.InvoiceRefundPayments)
                    .FirstOrDefaultAsync(m => m.Id == refundMaster.Id);

                await transaction.CommitAsync();
                return await GetInvoiceRefundMaster(refundMaster.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Critical database transaction failure while creating the refund.");
            }
        }

        // DELETE: api/InvoiceRefundMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceRefundMaster(long id)
        {
            var invoiceRefundMaster = await _context.InvoiceRefundMasters.FindAsync(id);
            if (invoiceRefundMaster == null)
            {
                return NotFound();
            }

            _context.InvoiceRefundMasters.Remove(invoiceRefundMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceRefundMasterExists(long id)
        {
            return _context.InvoiceRefundMasters.Any(e => e.Id == id);
        }

        // Helper mapper to keep clean structural separation
        private CreateInvoiceRefundDto MapToCreateInvoiceRefundDto(InvoiceRefundMaster im)
        {
            return new CreateInvoiceRefundDto
            {
                invoiceRefundMasterDto = MapToInvoiceRefundMasterDto(im),
                invoiceRefundDetailsDto = MapToInvoiceRefundDetailsDto(im.Id),
                invoiceRefundPaymentsDto = MapToInvoiceRefundPaymentsDto(im.Id)
            };
        }

        private InvoiceRefundMasterDto MapToInvoiceRefundMasterDto(InvoiceRefundMaster im)
        {
            return new InvoiceRefundMasterDto
            {
                Id = im.Id,
                tbirm_InvoiceRefundIdRad = im.tbirm_InvoiceRefundIdRad,
                tbirm_InvRefundDate = im.tbirm_InvRefundDate,
                tbirm_RefundType = im.tbirm_RefundType,
                tbirm_InvoiceId = im.tbirm_InvoiceId,
                tbirm_SubTotal = im.tbirm_SubTotal,
                tbirm_SaleTax = im.tbirm_SaleTax,
                tbirm_Labour = im.tbirm_Labour,
                tbirm_DisPer = im.tbirm_DisPer,
                tbirm_DisAmt = im.tbirm_DisAmt,
                tbirm_Total = im.tbirm_Total,
                tbirm_RefundAmt = im.tbirm_RefundAmt,
                tbirm_AdjAmt = im.tbirm_AdjAmt,
                tbirm_Note = im.tbirm_Note,
                tbirm_Delinfo = im.tbirm_Delinfo,
                tbirm_Item_Delete_after_Invoice_Refund_Create = im.tbirm_Item_Delete_after_Invoice_Refund_Create,
                UserName = im.UserName,
                SetDate = im.SetDate,
                OriginalInvoiceName = im.tbirm_Invoice != null ? im.tbirm_Invoice.tbim_Name : string.Empty,
                tbim_InvoiceIdRad = im.tbirm_Invoice != null ? im.tbirm_Invoice.tbim_InvoiceIdRad : null,
                tbim_Phone= im.tbirm_Invoice != null ? im.tbirm_Invoice.tbim_Phone : string.Empty,
                OriginalInvoiceDate = im.tbirm_Invoice != null ? (DateTime?)im.tbirm_Invoice.tbim_InvDate : null
            };
        }

        private List<InvoiceRefundDetailsDto> MapToInvoiceRefundDetailsDto(long Id)
        {
            var list = _context.InvoiceRefundDetails
                .Where(d => d.tbird_InvoiceRefundId == Id)
                .Select(d => new InvoiceRefundDetailsDto
                {
                    Id = d.Id,
                    tbird_InvoiceRefundId = d.tbird_InvoiceRefundId,
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
                    tbird_Taxable = d.tbird_Taxable,
                    tbird_UnitPrice = d.tbird_UnitPrice,
                    tbird_LineTotal = d.tbird_LineTotal,
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

        private List<InvoiceRefundPaymentsDto> MapToInvoiceRefundPaymentsDto(long Id)
        {
            var list = (from p in _context.InvoiceRefundPayments
                        join rm in _context.RefundMethodNames
                            on p.tbirp_RefundMethodId equals rm.Id into gj
                        from rm in gj.DefaultIfEmpty()
                        where p.tbirp_InvoiceRefundId == Id
                        select new InvoiceRefundPaymentsDto
                        {
                            Id = p.Id,
                            tbirp_InvoiceRefundId = p.tbirp_InvoiceRefundId,
                            tbirp_RefundMethodId = p.tbirp_RefundMethodId,
                            tbirp_RefundAmt = p.tbirp_RefundAmt,
                            tbirp_Date = p.tbirp_Date,
                            RefundMethodName = rm != null ? rm.tbrmn_RefundMethodName : string.Empty
                        })
                        .ToList();

            return list;
        }

        private InvoiceRefundMaster MapToInvoiceRefundMaster(InvoiceRefundMasterDto dto)
        {
            return new InvoiceRefundMaster
            {
                Id = dto.Id,
                tbirm_InvoiceRefundIdRad = dto.tbirm_InvoiceRefundIdRad,
                tbirm_InvRefundDate = dto.tbirm_InvRefundDate,
                tbirm_RefundType = dto.tbirm_RefundType,
                tbirm_InvoiceId = dto.tbirm_InvoiceId,
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
                tbirm_Item_Delete_after_Invoice_Refund_Create = dto.tbirm_Item_Delete_after_Invoice_Refund_Create,
                UserName = dto.UserName,
                SetDate = dto.SetDate
            };
        }

        private List<InvoiceRefundDetails> MapToInvoiceRefundDetails(List<InvoiceRefundDetailsDto> dtos)
        {
            var list = new List<InvoiceRefundDetails>();
            foreach (var dto in dtos)
            {
                var detail = new InvoiceRefundDetails
                {
                    Id = dto.Id,
                    tbird_InvoiceRefundId = dto.tbird_InvoiceRefundId,
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
                    tbird_Taxable = dto.tbird_Taxable,
                    tbird_UnitPrice = dto.tbird_UnitPrice,
                    tbird_LineTotal = dto.tbird_LineTotal,
                    tbird_TaxAmt = dto.tbird_TaxAmt
                };
                list.Add(detail);
            }
            return list;
        }

        private List<InvoiceRefundPayments> MapToInvoiceRefundPayments(List<InvoiceRefundPaymentsDto> dtos)
        {
            var list = new List<InvoiceRefundPayments>();
            foreach (var dto in dtos)
            {
                var payment = new InvoiceRefundPayments
                {
                    Id = dto.Id,
                    tbirp_InvoiceRefundId = dto.tbirp_InvoiceRefundId,
                    tbirp_RefundMethodId = dto.tbirp_RefundMethodId,
                    tbirp_RefundAmt = dto.tbirp_RefundAmt,
                    tbirp_Date = dto.tbirp_Date
                };
                list.Add(payment);
            }
            return list;
        }

        private void UpdateInvoiceRefundMaster(InvoiceRefundMaster im, InvoiceRefundMasterDto dto)
        {
            im.Id = dto.Id;
            //im.tbirm_InvoiceRefundIdRad = dto.tbirm_InvoiceRefundIdRad;
            im.tbirm_InvRefundDate = dto.tbirm_InvRefundDate;
            im.tbirm_RefundType = dto.tbirm_RefundType;
            im.tbirm_InvoiceId = dto.tbirm_InvoiceId;
            im.tbirm_SubTotal = dto.tbirm_SubTotal;
            im.tbirm_SaleTax = dto.tbirm_SaleTax;
            im.tbirm_Labour = dto.tbirm_Labour;
            im.tbirm_DisPer = dto.tbirm_DisPer;
            im.tbirm_DisAmt = dto.tbirm_DisAmt;
            im.tbirm_Total = dto.tbirm_Total;
            im.tbirm_RefundAmt = dto.tbirm_RefundAmt;
            im.tbirm_AdjAmt = dto.tbirm_AdjAmt;
            im.tbirm_Note = dto.tbirm_Note;
            im.tbirm_Delinfo = dto.tbirm_Delinfo;
            im.tbirm_Item_Delete_after_Invoice_Refund_Create = dto.tbirm_Item_Delete_after_Invoice_Refund_Create;
            im.UserName = dto.UserName;
            im.SetDate = dto.SetDate;
        }

        private void ProcessInvoiceRefundDetailsUpsert(InvoiceRefundMaster refundMaster, List<InvoiceRefundDetailsDto> incomingDetails)
        {
            // Purge removed records
            var incomingDetailIds = incomingDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = refundMaster.InvoiceRefundDetails.Where(d => !incomingDetailIds.Contains(d.Id)).ToList();
            _context.InvoiceRefundDetails.RemoveRange(detailsToRemove);

            Distributors distributor = new Distributors();
            // Upsert remaining records
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

                    refundMaster.InvoiceRefundDetails.Add(new InvoiceRefundDetails
                    {
                        tbird_InvoiceRefundId = refundMaster.Id,
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
                        tbird_Taxable = detailDto.tbird_Taxable,
                        tbird_UnitPrice = detailDto.tbird_UnitPrice,
                        tbird_LineTotal = detailDto.tbird_LineTotal,
                        tbird_TaxAmt = detailDto.tbird_TaxAmt
                    });
                }
                else
                {
                    var existingDetail = refundMaster.InvoiceRefundDetails.FirstOrDefault(d => d.Id == detailDto.Id);

                    var itemMaster = detailDto.tbird_ItemId.HasValue ? _context.ItemMasters.Find(detailDto.tbird_ItemId.Value) : null;
                    var dept = itemMaster != null ? _context.Departments.Find(itemMaster.tbim_ItemCategoryId) : null;

                    if (itemMaster != null && itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    if (existingDetail != null)
                    {
                        existingDetail.tbird_InvoiceRefundId = refundMaster.Id;
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
                        existingDetail.tbird_Taxable = detailDto.tbird_Taxable;
                        existingDetail.tbird_UnitPrice = detailDto.tbird_UnitPrice;
                        existingDetail.tbird_LineTotal = detailDto.tbird_LineTotal;
                        existingDetail.tbird_TaxAmt = detailDto.tbird_TaxAmt;
                    }
                }
            }
        }

        private void ProcessInvoiceRefundPaymentsUpsert(InvoiceRefundMaster refundMaster, List<InvoiceRefundPaymentsDto> incomingPayments)
        {
            // Purge removed payments
            var incomingPaymentIds = incomingPayments.Where(p => p.Id > 0).Select(p => p.Id).ToList();
            var paymentsToRemove = refundMaster.InvoiceRefundPayments.Where(p => !incomingPaymentIds.Contains(p.Id)).ToList();
            _context.InvoiceRefundPayments.RemoveRange(paymentsToRemove);

            // Upsert payments
            foreach (var payDto in incomingPayments)
            {
                if (payDto.Id == 0)
                {
                    refundMaster.InvoiceRefundPayments.Add(new InvoiceRefundPayments
                    {
                        tbirp_InvoiceRefundId = refundMaster.Id,
                        tbirp_RefundMethodId = payDto.tbirp_RefundMethodId,
                        tbirp_RefundAmt = payDto.tbirp_RefundAmt,
                        tbirp_Date = DateTime.UtcNow
                    });
                }
                else
                {
                    var existingPayment = refundMaster.InvoiceRefundPayments.FirstOrDefault(p => p.Id == payDto.Id);
                    if (existingPayment != null)
                    {
                        existingPayment.tbirp_InvoiceRefundId = refundMaster.Id;
                        existingPayment.tbirp_RefundMethodId = payDto.tbirp_RefundMethodId;
                        existingPayment.tbirp_RefundAmt = payDto.tbirp_RefundAmt;
                        existingPayment.tbirp_Date = payDto.tbirp_Date;
                    }
                }
            }
        }
    }
}