using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TireInventory.Data;
using TireInventory.Models;

namespace TireInventory.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceMasterController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public InvoiceMasterController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/InvoiceMaster
        [HttpGet]


        //public async Task<ActionResult<IEnumerable<InvoiceMasterDto>>> GetInvoiceMasters()
        //{
        //    var list = await _context.InvoiceMasters
        //        .Select(m => new InvoiceMasterDto
        //        {
        //            Id = m.Id,
        //            tbim_Phone = m.tbim_Phone,
        //            tbim_InvDate = m.tbim_InvDate,
        //            tbim_Name = m.tbim_Name,
        //            tbim_TaxId = m.tbim_TaxId,
        //            tbim_VehicleMake = m.tbim_VehicleMake,
        //            tbim_Model = m.tbim_Model,
        //            tbim_Year = m.tbim_Year,
        //            tbim_Odometer = m.tbim_Odometer,
        //            tbim_SubTotal = m.tbim_SubTotal,
        //            tbim_SaleTax = m.tbim_SaleTax,
        //            tbim_Labour = m.tbim_Labour,
        //            tbim_DisPer = m.tbim_DisPer,
        //            tbim_DisAmt = m.tbim_DisAmt,
        //            tbim_Total = m.tbim_Total,
        //            tbim_PaidAmt = m.tbim_PaidAmt,
        //            tbim_PayInfo = m.tbim_PayInfo,
        //            tbim_Note = m.tbim_Note,
        //            tbim_LocationDetailsId = m.tbim_LocationDetailsId,
        //            UserName = m.UserName,
        //            SetDate = m.SetDate,
        //            LocationName = m.tbim_LocationDetails != null ? m.tbim_LocationDetails.tbld_LocationName : string.Empty,
        //            TaxCompanyName = m.tbim_Tax != null ? m.tbim_Tax.tbti_ComName : string.Empty
        //        })
        //        .ToListAsync();

        //    return Ok(list);
        //}


        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreateInvoiceDto>>> GetInvoiceMasters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Fail-safe check to prevent massive memory overloads
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            // 1. Fetch only the paginated slice of Master Invoices first (Eager load lookups)
            var invoiceMasters = await _context.InvoiceMasters
                .OrderByDescending(m => m.tbim_InvDate) // Show newest invoices first
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2. Map each master record in memory using your instance mapping functions
            var dtos = new List<CreateInvoiceDto>();
            foreach (var im in invoiceMasters)
            {
                var dto = MapToCreateInvoiceDto(im);
                dtos.Add(dto);
            }

            // 3. Add pagination metadata headers so your Next.js SWR configuration knows total pages
            int totalRecords = await _context.InvoiceMasters.CountAsync();
            Response.Headers.Add("X-Total-Count", totalRecords.ToString());

            return Ok(dtos);
        }

        // GET: api/InvoiceMaster/5
        [HttpGet("{id}")]
        //public async Task<ActionResult<InvoiceMasterDto>> GetInvoiceMaster(long id)
        //{
        //    var dto = await _context.InvoiceMasters
        //        .Where(m => m.Id == id)
        //        .Select(m => new InvoiceMasterDto
        //        {
        //            Id = m.Id,
        //            tbim_Phone = m.tbim_Phone,
        //            tbim_InvDate = m.tbim_InvDate,
        //            tbim_Name = m.tbim_Name,
        //            tbim_TaxId = m.tbim_TaxId,
        //            tbim_VehicleMake = m.tbim_VehicleMake,
        //            tbim_Model = m.tbim_Model,
        //            tbim_Year = m.tbim_Year,
        //            tbim_Odometer = m.tbim_Odometer,
        //            tbim_SubTotal = m.tbim_SubTotal,
        //            tbim_SaleTax = m.tbim_SaleTax,
        //            tbim_Labour = m.tbim_Labour,
        //            tbim_DisPer = m.tbim_DisPer,
        //            tbim_DisAmt = m.tbim_DisAmt,
        //            tbim_Total = m.tbim_Total,
        //            tbim_PaidAmt = m.tbim_PaidAmt,
        //            tbim_PayInfo = m.tbim_PayInfo,
        //            tbim_Note = m.tbim_Note,
        //            tbim_LocationDetailsId = m.tbim_LocationDetailsId,
        //            UserName = m.UserName,
        //            SetDate = m.SetDate,
        //            LocationName = m.tbim_LocationDetails != null ? m.tbim_LocationDetails.tbld_LocationName : string.Empty,
        //            TaxCompanyName = m.tbim_Tax != null ? m.tbim_Tax.tbti_ComName : string.Empty
        //        })
        //        .FirstOrDefaultAsync();

        //    if (dto == null) return NotFound();
        //    return Ok(dto);
        //}
        
        public async Task<ActionResult<CreateInvoiceDto>> GetInvoiceMaster(long id)
        {
            var invoiceMaster = await _context.InvoiceMasters
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (invoiceMaster == null) return NotFound();
            // Now execute mappings safely in memory using regular instance methods
            var dto = MapToCreateInvoiceDto(invoiceMaster);
            return Ok(dto);
        }


        // GET: api/InvoiceMaster/{id}/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<IEnumerable<InvoiceDetailsDto>>> GetInvoiceDetailsByInvoice(long id)
        {
            var list = await _context.InvoiceDetails
                .Where(d => d.tbid_InvoiceId == id)
                .Select(d => new InvoiceDetailsDto
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
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/InvoiceMaster/{id}/payments
        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<InvoicePaymentsDto>>> GetInvoicePaymentsByInvoice(long id)
        {
            var list = await (from p in _context.InvoicePayments
                              join pay in _context.PaymentNames
                                  on p.tbip_PaymentId equals pay.Id into gj
                              from pay in gj.DefaultIfEmpty()
                              where p.tbip_InvoiceId == id
                              select new InvoicePaymentsDto
                              {
                                  Id = p.Id,
                                  tbip_InvoiceId = p.tbip_InvoiceId,
                                  tbip_PaymentId = p.tbip_PaymentId,
                                  tbip_PayAmt = p.tbip_PayAmt,
                                  tbip_Date = p.tbip_Date,
                                  tbip_PaymentTypeId = p.tbip_PaymentTypeId,
                                  PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                              })
                             .ToListAsync();

            return Ok(list);
        }


        // PUT: api/InvoiceMaster/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceMaster(long id, InvoiceMaster invoiceMaster)
        {
            if (id != invoiceMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceMasterExists(id))
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

        // POST: api/InvoiceMaster
        [HttpPost]
        public async Task<ActionResult<CreateInvoiceDto>> PostInvoiceMaster(InvoiceMaster invoiceMaster)
        {
            _context.InvoiceMasters.Add(invoiceMaster);
            await _context.SaveChangesAsync();

            return await GetInvoiceMaster(invoiceMaster.Id);
            //return CreatedAtAction("GetInvoiceMaster", new { id = invoiceMaster.Id }, invoiceMaster);
        }

        // POST: api/InvoiceMaster/CreateInvoice
        // Accepts a master, a list of details and a list of payments.
        // Inserts master first, then inserts details and payments with FK set to master.Id
        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<CreateInvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
        {
            if (createInvoiceDto == null) return BadRequest();

            var invoiceMaster = MapToInvoiceMaster( createInvoiceDto.invoiceMasterDto ?? new InvoiceMasterDto());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add master first to obtain generated Id
                _context.InvoiceMasters.Add(invoiceMaster);
                await _context.SaveChangesAsync();

                // DETAILS: set FK and enrich from ItemMaster/Departments/Distributors as before
                var details = MapToInvoiceDetails(createInvoiceDto.invoiceDetailsDto ?? new List<InvoiceDetailsDto>());
                foreach (var d in details)
                {
                    d.tbid_InvoiceId = invoiceMaster.Id;
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
                    _context.InvoiceDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                // PAYMENTS: set FK to created master Id and insert
                var payments = MapToInvoicePayments(createInvoiceDto.invoicePaymentsDto ?? new List<InvoicePaymentsDto>());
                foreach (var p in payments)
                {
                    p.tbip_InvoiceId = invoiceMaster.Id;
                    p.Id = 0;
                    p.tbip_Invoice = null;
                }

                if (payments.Count > 0)
                {
                    _context.InvoicePayments.AddRange(payments);
                    await _context.SaveChangesAsync();
                }

                // Optionally load created invoice with details and payments to return
                var created = await _context.InvoiceMasters
                    .Include(m => m.tbl_Invoice_Details)
                    .Include(m => m.tbl_Invoice_Payments)
                    .FirstOrDefaultAsync(m => m.Id == invoiceMaster.Id);

                await transaction.CommitAsync();
                return await GetInvoiceMaster(invoiceMaster.Id);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Critical database transaction failure while placing the order.");
            }

            
        }

        // DELETE: api/InvoiceMaster/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceMaster(long id)
        {
            var invoiceMaster = await _context.InvoiceMasters.FindAsync(id);
            if (invoiceMaster == null)
            {
                return NotFound();
            }

            _context.InvoiceMasters.Remove(invoiceMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceMasterExists(long id)
        {
            return _context.InvoiceMasters.Any(e => e.Id == id);
        }

        // Helper mapper to keep clean structural separation
        private CreateInvoiceDto MapToCreateInvoiceDto(InvoiceMaster im)
        {
            return new CreateInvoiceDto
            {
                invoiceMasterDto = MapToInvoiceMasterDto(im),
                invoiceDetailsDto = MapToInvoiceDetailsDto(im.Id),
                invoicePaymentsDto= MapToInvoicePaymentsDto(im.Id)
            };
        }

        private InvoiceMasterDto MapToInvoiceMasterDto(InvoiceMaster invoiceMaster)
        {
            return new InvoiceMasterDto
            {
                Id = invoiceMaster.Id,
                tbim_Phone = invoiceMaster.tbim_Phone,
                tbim_InvDate = invoiceMaster.tbim_InvDate,
                tbim_Name = invoiceMaster.tbim_Name,
                tbim_TaxId = invoiceMaster.tbim_TaxId,
                tbim_VehicleMake = invoiceMaster.tbim_VehicleMake,
                tbim_Model = invoiceMaster.tbim_Model,
                tbim_Year = invoiceMaster.tbim_Year,
                tbim_Odometer = invoiceMaster.tbim_Odometer,
                tbim_SubTotal = invoiceMaster.tbim_SubTotal,
                tbim_SaleTax = invoiceMaster.tbim_SaleTax,
                tbim_Labour = invoiceMaster.tbim_Labour,
                tbim_DisPer = invoiceMaster.tbim_DisPer,
                tbim_DisAmt = invoiceMaster.tbim_DisAmt,
                tbim_Total = invoiceMaster.tbim_Total,
                tbim_PaidAmt = invoiceMaster.tbim_PaidAmt,
                tbim_PayInfo = invoiceMaster.tbim_PayInfo,
                tbim_Note = invoiceMaster.tbim_Note,
                tbim_LocationDetailsId = invoiceMaster.tbim_LocationDetailsId,
                UserName = invoiceMaster.UserName,
                SetDate = invoiceMaster.SetDate,
                LocationName = invoiceMaster.tbim_LocationDetails != null ? invoiceMaster.tbim_LocationDetails.tbld_LocationName : string.Empty,
                TaxCompanyName = invoiceMaster.tbim_Tax != null ? invoiceMaster.tbim_Tax.tbti_ComName : string.Empty
            };
        }


        private List<InvoiceDetailsDto> MapToInvoiceDetailsDto(long Id)
        {
            var list = _context.InvoiceDetails
                .Where(d => d.tbid_InvoiceId == Id)
                .Select(d => new InvoiceDetailsDto
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

        private List<InvoicePaymentsDto> MapToInvoicePaymentsDto(long Id)
        {
            var list = (from p in _context.InvoicePayments
                             join pay in _context.PaymentNames
                                 on p.tbip_PaymentId equals pay.Id into gj
                             from pay in gj.DefaultIfEmpty()
                             where p.tbip_InvoiceId == Id
                             select new InvoicePaymentsDto
                             {
                                 Id = p.Id,
                                 tbip_InvoiceId = p.tbip_InvoiceId,
                                 tbip_PaymentId = p.tbip_PaymentId,
                                 tbip_PayAmt = p.tbip_PayAmt,
                                 tbip_Date = p.tbip_Date,
                                 tbip_PaymentTypeId = p.tbip_PaymentTypeId,
                                 tbip_LayawayId = p.tbip_LayawayId,
                                 tdip_fromlayaway = p.tdip_fromlayaway,
                                 tbip_LayawayDate = p.tbip_LayawayDate,
                                 PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                             })
                             .ToList();

            return list;

        }

        private InvoiceMaster MapToInvoiceMaster(InvoiceMasterDto dto)
        {
            return new InvoiceMaster
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
                tbim_Item_Delete_after_Invoice_Create = dto.tbim_Item_Delete_after_Invoice_Create,
                tbim_LaywayNo = dto.tbim_LaywayNo,
                tbim_LaywayDate = dto.tbim_LaywayDate,
                UserName = dto.UserName,
                SetDate = dto.SetDate,
                tbim_Left_Front = dto.tbim_Left_Front,
                tbim_Right_Front = dto.tbim_Right_Front,
                tbim_Left_Rear = dto.tbim_Left_Rear,
                tbim_Right_Rear = dto.tbim_Right_Rear,
                tbim_EmailAddress = dto.tbim_EmailAddress,
                tbim_IDNo = dto.tbim_IDNo,
                tbim_RefundType = dto.tbim_RefundType,
                tbim_LocationDetailsId = dto.tbim_LocationDetailsId
            };
        }

        private List<InvoiceDetails> MapToInvoiceDetails(List<InvoiceDetailsDto> dtos)
        {
            var list = new List<InvoiceDetails>();
            foreach (var dto in dtos)
            {
                var detail = new InvoiceDetails
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

        private List<InvoicePayments> MapToInvoicePayments(List<InvoicePaymentsDto> dtos)
        {
            var list = new List<InvoicePayments>();
            foreach (var dto in dtos)
            {
                var payment = new InvoicePayments
                {
                    Id = dto.Id,
                    tbip_InvoiceId = dto.tbip_InvoiceId,
                    tbip_PaymentId = dto.tbip_PaymentId,
                    tbip_PayAmt = dto.tbip_PayAmt,
                    tbip_Date = dto.tbip_Date,
                    tbip_PaymentTypeId = dto.tbip_PaymentTypeId,
                    tbip_LayawayId= dto.tbip_LayawayId,
                    tdip_fromlayaway= dto.tdip_fromlayaway,
                    tbip_LayawayDate = dto.tbip_LayawayDate
                };
                list.Add(payment);
            }
            return list;
        }

    }
}