using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Bill.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _billRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public BillService(IBillRepository billRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _billRepository = billRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Bill> CreateBill(AddBillRequest request)
        {
            try
            {
                var bill = new Bill
                {
                    Ammount = request.Ammount,
                    UserTypeId = request.UserTypeId,
                    UserId = request.UserId,
                    UserSellerId = request.UserSellerId,
                    GymId = request.GymId,
                    Ip = request.Ip
                };
                var created = _billRepository.CreateBill(bill);
                return new ApplicationResponse<Bill>
                {
                    Success = true,
                    Message = "Factura creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Bill>
                {
                    Success = false,
                    Message = "Error técnico al crear la factura.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Bill> GetBillById(Guid id)
        {
            var bill = _billRepository.GetBillById(id);
            if (bill == null)
            {
                return new ApplicationResponse<Bill>
                {
                    Success = false,
                    Message = "Factura no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Bill>
            {
                Success = true,
                Data = bill
            };
        }

        public ApplicationResponse<IEnumerable<Bill>> GetAllBills()
        {
            var bills = _billRepository.GetAllBills();
            return new ApplicationResponse<IEnumerable<Bill>>
            {
                Success = true,
                Data = bills
            };
        }

        public ApplicationResponse<bool> UpdateBill(UpdateBillRequest request)
        {
            try
            {
                var billBefore = _billRepository.GetBillById(request.Id);
                var bill = new Bill
                {
                    Id = request.Id,
                    Ammount = request.Ammount,
                    UserTypeId = request.UserTypeId,
                    UserId = request.UserId,
                    UserSellerId = request.UserSellerId,
                    GymId = request.GymId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _billRepository.UpdateBill(bill);
                if (updated)
                {
                    _logChangeService.LogChange("Bill", billBefore, bill.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Factura actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la factura (no encontrada o inactiva).",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar la factura.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteBill(Guid id)
        {
            try
            {
                var deleted = _billRepository.DeleteBill(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Factura eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Factura no encontrada o ya eliminada.",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar la factura.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Bill>> FindBillsByFields(Dictionary<string, object> filters)
        {
            var bills = _billRepository.FindBillsByFields(filters);
            return new ApplicationResponse<IEnumerable<Bill>>
            {
                Success = true,
                Data = bills
            };
        }
    }
}
