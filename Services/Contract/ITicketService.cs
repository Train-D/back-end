﻿using Train_D.DTO.TicketDTO;


namespace Train_D.Services.Contract
{
    public interface ITicketService
    {
        bool Isvaild(string pay);
        Task<bool> IsExist(TicketBookRequest dto);
        Task<TicketDTO> Book(TicketBookRequest dto, string userId, string username);
        Task<IEnumerable<TicketDTO>> GetTickets(string userId,string username);
        Task<TrackingResponse> getTrackingInfo(int ticketId);
        Task<string> CancelTicket(int TicketId);
        Task<string> GetPaymentId(int ticketId);
    }
}
