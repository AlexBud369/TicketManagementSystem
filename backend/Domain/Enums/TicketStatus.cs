namespace Domain.Enums;

public enum TicketStatus {
    Active = 0,
    Used = 1,
    Cancelled = 2,
    Refunded = 3
}

public enum TicketScanStatus {
    Valid = 0,
    AlreadyUsed = 1,
    Invalid = 2
}
