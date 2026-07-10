using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Hosting;
using TireInventory.Data;
using TireInventory.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<ActionResult<IEnumerable<CreateInvoiceDto>>> GetInvoiceMasters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Fail-safe check to prevent massive memory overloads
            if (pageSize > 100) pageSize = 100;
            if (pageNumber < 1) pageNumber = 1;

            // 1. Fetch only the paginated slice of Master Invoices first (Eager load lookups)
            var invoiceMasters = await _context.InvoiceMasters
                .Include(m => m.InvoiceDetails)
                .Include(m => m.InvoicePayments)
                    .ThenInclude(m => m.tbip_Payment)
                .Include(m => m.InvoiceRefundMasters)
                .Include(m => m.tbim_Tax)
                .Include(m => m.tbim_LocationDetails)
                .OrderByDescending(m => m.tbim_InvDate) // Show newest invoices first
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2. Map each master record in memory using your instance mapping functions
            var dtos = new List<CreateInvoiceDto>();
            foreach (var im in invoiceMasters)
            {
                var dto = MapToCreateInvoiceDto(im);
                //TestMapping(im); // Optional: for debugging or logging purposes
                dtos.Add(dto);
            }

            // 3. Add pagination metadata headers so your Next.js SWR configuration knows total pages
            int totalRecords = await _context.InvoiceMasters.CountAsync();
            Response.Headers.Add("X-Total-Count", totalRecords.ToString());


            

            return Ok(dtos);
        }

        // GET: api/InvoiceMaster/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreateInvoiceDto>> GetInvoiceMaster(long id)
        {
            var invoiceMaster = await _context.InvoiceMasters
                .Include(m => m.InvoiceDetails)
                .Include(m => m.InvoicePayments)
                    .ThenInclude(m=>m.tbip_Payment)
                .Include(m => m.InvoiceRefundMasters)
                .Include(m => m.tbim_Tax)
                .Include(m => m.tbim_LocationDetails)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (invoiceMaster == null) return NotFound();
            // Now execute mappings safely in memory using regular instance methods
            var dto = MapToCreateInvoiceDto(invoiceMaster);
            return Ok(dto);
        }


        // PUT: api/InvoiceMaster/5
        //[HttpPut("{id}")]

        /*
        1. The Strategy: "Upsert & Purge" Pattern
        The most robust way to handle this in a REST API without creating messy, stateful logic is the Upsert & Purge pattern.
        Instead of tracking what the user clicked on the frontend, your Next.js application simply sends the entire current state of the invoice details and payments back to the API. The API then compares what is in the database with what the user sent:
        - If a row has an Id > 0, Update it.
        - If a row has an Id == 0, Insert it.
        - Any row currently in the database that is missing from the payload is Deleted.

        The Tracking State: When you fetch the InvoiceMaster from the database using .Include(m => m.InvoiceDetails), EF Core loads those records into memory and starts "tracking" them. It keeps a secret snapshot copy of what the data looked like the moment it was loaded.
        In-Memory Modification: When you loop through your list and write existingDetail.tbid_Qty = detailDto.tbid_Qty;, you are directly modifying that object in the server's RAM.
        The Detect Changes Phase: When you finally call await _context.SaveChangesAsync(), EF Core automatically triggers its Change Tracker. It scans all tracked objects in memory and compares them line-by-line against their original snapshots.

        / Purge Step: Remove payments deleted in UI
        var incomingPaymentIds = dto.InvoicePayments.Where(p => p.Id > 0).Select(p => p.Id).ToList();
        var paymentsToRemove = invoiceMaster.InvoicePayments.Where(p => !incomingPaymentIds.Contains(p.Id)).ToList();
        _context.InvoicePayments.RemoveRange(paymentsToRemove);
        */

        // POST: api/InvoiceMaster/EditInvoice
        [HttpPost("EditInvoice")]
        public async Task<IActionResult> EditInvoice(long id, CreateInvoiceDto createInvoiceDto)
        {
            if (createInvoiceDto == null) return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Fetch the existing master record along with current line details and payments
                var invoiceMaster = await _context.InvoiceMasters
                .Include(m => m.InvoiceDetails)
                .Include(m => m.InvoicePayments)
                .Include(m => m.tbim_Tax)
                .Include(m => m.tbim_LocationDetails)
                .Include(m => m.InvoicePayments)
                .FirstOrDefaultAsync(m => m.Id == id);

                // Update invoiceMaster
                UpdateInvoiceMaster(invoiceMaster, createInvoiceDto.invoiceMasterDto ?? new InvoiceMasterDto());
                _context.Entry(invoiceMaster).State = EntityState.Modified;

                // 2. Process Details update
                ProcessInvoiceDetailsUpsert(invoiceMaster, createInvoiceDto.invoiceDetailsDto ?? new List<InvoiceDetailsDto>());
                // 3. Process Payments update
                ProcessInvoicePaymentsUpsert(invoiceMaster, createInvoiceDto.invoicePaymentsDto ?? new List<InvoicePaymentsDto>());

                // Save everything atomically
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "Invoice adjusted successfully", InvoiceId = id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the exception here (e.g., _logger.LogError(ex, "Error updating invoice..."))
                //return StatusCode(500, "An error occurred while updating the invoice tracking entities.");
                return StatusCode(500, ex.InnerException.Message.ToString());
            }
        }

        // POST: api/InvoiceMaster/CreateInvoice
        // Accepts a master, a list of details and a list of payments.
        // Inserts master first, then inserts details and payments with FK set to master.Id
        [HttpPost("CreateInvoice")]
        public async Task<ActionResult<CreateInvoiceDto>> CreateInvoice(CreateInvoiceDto createInvoiceDto)
        {
            if (createInvoiceDto == null) return BadRequest();

            var invoiceMaster = MapToInvoiceMaster( createInvoiceDto.invoiceMasterDto ?? new InvoiceMasterDto());
            invoiceMaster.tbim_InvoiceIdRad = GenerateTransactionID();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add master first to obtain generated Id
                _context.InvoiceMasters.Add(invoiceMaster);
                // Save to get InvoiceMaster add that will be the foreign key to details and payment table
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
                    // No need to save now, cause it's id is not going to be used in any of table.
                    // Will save later
                    //await _context.SaveChangesAsync();
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
                    // No need to save now, cause it's id is not going to be used in any of table.
                    // Will save later
                    //await _context.SaveChangesAsync();
                }


                //Save all changes made in the context
                await _context.SaveChangesAsync();
                // Optionally load created invoice with details and payments to return
                var created = await _context.InvoiceMasters
                    .Include(m => m.InvoiceDetails)
                    .Include(m => m.InvoicePayments)
                    .FirstOrDefaultAsync(m => m.Id == invoiceMaster.Id);

                await transaction.CommitAsync();
                return await GetInvoiceMaster(invoiceMaster.Id);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                //return StatusCode(501, "Critical database transaction failure while placing the order.");
                return StatusCode(500, ex.InnerException.Message.ToString());
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

        private void TestMapping(InvoiceMaster invoiceMaster)
        {
           var testDto=_context.InvoicePayments.Where(p => p.tbip_InvoiceId == invoiceMaster.Id)
                .Select(p => new InvoicePaymentsDto
                {
                    Id = p.Id,
                    tbip_InvoiceId = p.tbip_InvoiceId,
                    tbip_PaymentId = p.tbip_PaymentId,
                    tbip_PayAmt = p.tbip_PayAmt,
                    tbip_Date = p.tbip_Date,
                    tbip_PaymentType = p.tbip_PaymentType,
                    tbip_LayawayId = p.tbip_LayawayId,
                    tdip_fromlayaway = p.tdip_fromlayaway,
                    tbip_LayawayDate = p.tbip_LayawayDate
                })
                .ToList();  

        }
        private InvoiceMasterDto MapToInvoiceMasterDto(InvoiceMaster invoiceMaster)
        {
            return new InvoiceMasterDto
            {
                Id = invoiceMaster.Id,
                tbim_InvoiceIdRad = invoiceMaster.tbim_InvoiceIdRad,
                tbim_Phone = invoiceMaster.tbim_Phone,
                tbim_InvDate = invoiceMaster.tbim_InvDate,
                tbim_Name = invoiceMaster.tbim_Name,
                tbim_TaxId = invoiceMaster.tbim_TaxId,
                tbim_VehicleMake = invoiceMaster.tbim_VehicleMake,
                tbim_Model = invoiceMaster.tbim_Model,
                tbim_Year = invoiceMaster.tbim_Year,
                tbim_Odometer = invoiceMaster.tbim_Odometer,
                tbim_TreadDepth = invoiceMaster.tbim_TreadDepth,
                tbim_License = invoiceMaster.tbim_License,
                tbim_SubTotal = invoiceMaster.tbim_SubTotal,
                tbim_SaleTax = invoiceMaster.tbim_SaleTax,
                tbim_Labour = invoiceMaster.tbim_Labour,
                tbim_DisPer = invoiceMaster.tbim_DisPer,
                tbim_DisAmt = invoiceMaster.tbim_DisAmt,
                tbim_Total = invoiceMaster.tbim_Total,
                tbim_PaidAmt = invoiceMaster.tbim_PaidAmt,
                tbim_AdjAmt = invoiceMaster.tbim_AdjAmt,
                tbim_AdjTotal = invoiceMaster.tbim_AdjTotal,
                tbim_PayInfo = invoiceMaster.tbim_PayInfo,
                tbim_Note = invoiceMaster.tbim_Note,
                tbim_Delinfo = invoiceMaster.tbim_Delinfo,
                tbim_CompanyName = invoiceMaster.tbim_CompanyName,
                tbim_CompanyAddress = invoiceMaster.tbim_CompanyAddress,
                tbim_Item_Delete_after_Invoice_Create = invoiceMaster.tbim_Item_Delete_after_Invoice_Create,
                tbim_LaywayNo = invoiceMaster.tbim_LaywayNo,
                tbim_LaywayDate = invoiceMaster.tbim_LaywayDate,
                UserName = invoiceMaster.UserName,
                SetDate = invoiceMaster.SetDate,
                tbim_Left_Front = invoiceMaster.tbim_Left_Front,
                tbim_Right_Front = invoiceMaster.tbim_Right_Front,
                tbim_Left_Rear = invoiceMaster.tbim_Left_Rear,
                tbim_Right_Rear = invoiceMaster.tbim_Right_Rear,
                tbim_EmailAddress = invoiceMaster.tbim_EmailAddress,
                tbim_IDNo = invoiceMaster.tbim_IDNo,
                tbim_RefundType = invoiceMaster.tbim_RefundType,
                tbim_LocationDetailsId = invoiceMaster.tbim_LocationDetailsId,
                LayawayRefund=invoiceMaster.tbim_LaywayNo!=null & invoiceMaster.tbim_LaywayNo>0?
                              MapToInvoicePaymentsDtoByLayawayId(invoiceMaster.tbim_LaywayNo)
                              :new List<InvoicePaymentsDto>(),
                RefundAmount = invoiceMaster.InvoiceRefundMasters != null & invoiceMaster.InvoiceRefundMasters.Count>0 ? invoiceMaster.InvoiceRefundMasters.FirstOrDefault().tbirm_RefundAmt : (decimal)0.00,
                LocationName = invoiceMaster.tbim_LocationDetails != null ? invoiceMaster.tbim_LocationDetails.tbld_LocationName : string.Empty,
                TaxCompanyName = invoiceMaster.tbim_Tax != null ? invoiceMaster.tbim_Tax.tbti_ComName : string.Empty,
                TaxIdentificationNumber = invoiceMaster.tbim_Tax != null ? invoiceMaster.tbim_Tax.tbti_TaxNumber : string.Empty,
                TaxAddress = invoiceMaster.tbim_Tax != null ? invoiceMaster.tbim_Tax.tbti_Address : string.Empty,
                TaxPhone = invoiceMaster.tbim_Tax != null ? invoiceMaster.tbim_Tax.tbti_Phone : string.Empty,
                PaymentMethodName=string.Join(",", invoiceMaster.InvoicePayments.Select(p=>p.tbip_Payment.tbpn_PaymentName).Distinct().ToList()) ?? string.Empty
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
                                 tbip_PaymentType = p.tbip_PaymentType,
                                 tbip_LayawayId = p.tbip_LayawayId,
                                 tdip_fromlayaway = p.tdip_fromlayaway,
                                 tbip_LayawayDate = p.tbip_LayawayDate,
                                 PaymentName = pay != null ? pay.tbpn_PaymentName : string.Empty
                             })
                             .ToList();

            return list;

        } 
        
        private List<InvoicePaymentsDto> MapToInvoicePaymentsDtoByLayawayId(long? Id)
        {
            var list = (from p in _context.InvoicePayments
                             join pay in _context.PaymentNames
                                 on p.tbip_PaymentId equals pay.Id into gj
                             from pay in gj.DefaultIfEmpty()
                             where p.tbip_LayawayId == Id
                             select new InvoicePaymentsDto
                             {
                                 Id = p.Id,
                                 tbip_InvoiceId = p.tbip_InvoiceId,
                                 tbip_PaymentId = p.tbip_PaymentId,
                                 tbip_PayAmt = p.tbip_PayAmt,
                                 tbip_Date = p.tbip_Date,
                                 tbip_PaymentType = p.tbip_PaymentType,
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
                    tbip_PaymentType = dto.tbip_PaymentType,
                    tbip_LayawayId= dto.tbip_LayawayId,
                    tdip_fromlayaway= dto.tdip_fromlayaway,
                    tbip_LayawayDate = dto.tbip_LayawayDate
                };
                list.Add(payment);
            }
            return list;
        }

        private void UpdateInvoiceMaster(InvoiceMaster im,InvoiceMasterDto dto)
        {
            im.Id = dto.Id;
            //im.tbim_InvoiceIdRad = dto.tbim_InvoiceIdRad;
            im.tbim_Phone = dto.tbim_Phone;
            im.tbim_InvDate = dto.tbim_InvDate;
            im.tbim_Name = dto.tbim_Name;
            im.tbim_TaxId = dto.tbim_TaxId;
            im.tbim_VehicleMake = dto.tbim_VehicleMake;
            im.tbim_Model = dto.tbim_Model;
            im.tbim_Year = dto.tbim_Year;
            im.tbim_Odometer = dto.tbim_Odometer;
            im.tbim_TreadDepth = dto.tbim_TreadDepth;
            im.tbim_License = dto.tbim_License;
            im.tbim_SubTotal = dto.tbim_SubTotal;
            im.tbim_SaleTax = dto.tbim_SaleTax;
            im.tbim_Labour = dto.tbim_Labour;
            im.tbim_DisPer = dto.tbim_DisPer;
            im.tbim_DisAmt = dto.tbim_DisAmt;
            im.tbim_Total = dto.tbim_Total;
            im.tbim_PaidAmt = dto.tbim_PaidAmt;
            im.tbim_AdjAmt = dto.tbim_AdjAmt;
            im.tbim_AdjTotal = dto.tbim_AdjTotal;
            im.tbim_PayInfo = dto.tbim_PayInfo;
            im.tbim_Note = dto.tbim_Note;
            im.tbim_Delinfo = dto.tbim_Delinfo;
            im.tbim_CompanyName = dto.tbim_CompanyName;
            im.tbim_CompanyAddress = dto.tbim_CompanyAddress;
            im.tbim_Item_Delete_after_Invoice_Create = dto.tbim_Item_Delete_after_Invoice_Create;
            im.tbim_LaywayNo = dto.tbim_LaywayNo;
            im.tbim_LaywayDate = dto.tbim_LaywayDate;
            im.UserName = dto.UserName;
            im.SetDate = dto.SetDate;
            im.tbim_Left_Front = dto.tbim_Left_Front;
            im.tbim_Right_Front = dto.tbim_Right_Front;
            im.tbim_Left_Rear = dto.tbim_Left_Rear;
            im.tbim_Right_Rear = dto.tbim_Right_Rear;
            im.tbim_EmailAddress = dto.tbim_EmailAddress;
            im.tbim_IDNo = dto.tbim_IDNo;
            im.tbim_RefundType = dto.tbim_RefundType;
            im.tbim_LocationDetailsId = dto.tbim_LocationDetailsId;

        }

        private void ProcessInvoiceDetailsUpsert(InvoiceMaster invoiceMaster, List<InvoiceDetailsDto> incomingDetails)
        {
            // Purge removed records
            var incomingDetailIds = incomingDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = invoiceMaster.InvoiceDetails.Where(d => !incomingDetailIds.Contains(d.Id)).ToList();
            _context.InvoiceDetails.RemoveRange(detailsToRemove);

            Distributors distributor = new Distributors();
            // Upsert remaining records
            foreach (var detailDto in incomingDetails)
            {

                if (detailDto.Id == 0)
                {
                    var itemMaster = _context.ItemMasters.Find(detailDto.tbid_ItemId.Value);
                    var dept = _context.Departments.Find(itemMaster.tbim_ItemCategoryId);

                    if (itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    invoiceMaster.InvoiceDetails.Add(new InvoiceDetails
                    {
                        tbid_InvoiceId = invoiceMaster.Id,
                        tbid_ItemId = detailDto.tbid_ItemId,
                        //tbid_ItemCategory = detailDto.tbid_ItemCategory,
                        tbid_ItemCategory = itemMaster==null?null: (long?)itemMaster.tbim_ItemCategoryId,
                        //tbid_DepartmentName = detailDto.tbid_DepartmentName,
                        tbid_DepartmentName= dept?.Tbid_DepartmentName,
                        tbid_Size = detailDto.tbid_Size,
                        tbid_Brand = detailDto.tbid_Brand,
                        tbid_Series = detailDto.tbid_Series,
                        tbid_Bolt = detailDto.tbid_Bolt,
                        tbid_HoleS = detailDto.tbid_HoleS,
                        tbid_Zone = detailDto.tbid_Zone,
                        tbid_DistributorId = detailDto.tbid_DistributorId,
                        //tbid_DistributorName = detailDto.tbid_DistributorName,
                        tbid_DistributorName= distributor?.Name,
                        tbid_Qty = detailDto.tbid_Qty,
                        tbid_Taxable = detailDto.tbid_Taxable,
                        tbid_UnitPrice = detailDto.tbid_UnitPrice,
                        tbid_LineTotal = detailDto.tbid_LineTotal,
                        tbid_TaxAmt = detailDto.tbid_TaxAmt
                    });
                }
                else
                {
                    var existingDetail = invoiceMaster.InvoiceDetails.FirstOrDefault(d => d.Id == detailDto.Id);

                    var itemMaster = _context.ItemMasters.Find(detailDto.tbid_ItemId.Value);
                    var dept = _context.Departments.Find(itemMaster.tbim_ItemCategoryId);

                    if (itemMaster.tbim_DistributorId.HasValue)
                    {
                        distributor = _context.Distributors.Find(itemMaster.tbim_DistributorId.Value);
                    }

                    if (existingDetail != null)
                    {
                        existingDetail.tbid_InvoiceId = invoiceMaster.Id;
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


        private void ProcessInvoicePaymentsUpsert(InvoiceMaster invoiceMaster, List<InvoicePaymentsDto> incomingPayments)
        {
            // Purge removed payments
            var incomingPaymentIds = incomingPayments.Where(p => p.Id > 0).Select(p => p.Id).ToList();
            var paymentsToRemove = invoiceMaster.InvoicePayments.Where(p => !incomingPaymentIds.Contains(p.Id)).ToList();
            _context.InvoicePayments.RemoveRange(paymentsToRemove);

            // Upsert payments
            foreach (var payDto in incomingPayments)
            {
                if (payDto.Id == 0)
                {
                    invoiceMaster.InvoicePayments.Add(new InvoicePayments
                    {
                        tbip_InvoiceId = invoiceMaster.Id,
                        tbip_PaymentId = payDto.tbip_PaymentId,
                        tbip_PayAmt = payDto.tbip_PayAmt,
                        tbip_Date = DateTime.UtcNow,
                        tbip_PaymentType = payDto.tbip_PaymentType,
                        tbip_LayawayId = payDto.tbip_LayawayId,
                        tdip_fromlayaway = payDto.tdip_fromlayaway,
                        tbip_LayawayDate = payDto.tbip_LayawayDate
                    });
                }
                else
                {
                    var existingPayment = invoiceMaster.InvoicePayments.FirstOrDefault(p => p.Id == payDto.Id);
                    if (existingPayment != null)
                    {
                        existingPayment.tbip_InvoiceId = invoiceMaster.Id;
                        existingPayment.tbip_PaymentId = payDto.tbip_PaymentId;
                        existingPayment.tbip_PayAmt = payDto.tbip_PayAmt;
                        existingPayment.tbip_Date = payDto.tbip_Date;
                        existingPayment.tbip_PaymentType = payDto.tbip_PaymentType;
                        existingPayment.tbip_LayawayId = payDto.tbip_LayawayId;
                        existingPayment.tdip_fromlayaway = payDto.tdip_fromlayaway;
                        existingPayment.tbip_LayawayDate = payDto.tbip_LayawayDate;
                    }
                }
            }
        }

        private long? GenerateTransactionID()
        {
            // 1. Get current time as HHmmss
            string timePart = DateTime.Now.ToString("HHmmss");

            // 2. Generate random number up to 1,000,000
            int randomPart = Random.Shared.Next(0, 1000000);

            // 3. Combine them
            return (long?)Convert.ToInt64( $"{timePart}{randomPart}");
        }

    }
}