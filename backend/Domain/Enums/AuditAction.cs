namespace Domain.Enums;

public enum AuditAction {
    Login = 0,
    Logout = 1,
    PasswordReset = 2,
    PasswordChanged = 3,

    UserCreated = 10,
    UserUpdated = 11,
    UserDeleted = 12,
    UserDisabled = 13,
    UserEnabled = 14,
    RoleChanged = 15,

    EventCreated = 20,
    EventUpdated = 21,
    EventDeleted = 22,
    EventPublished = 23,
    EventUnpublished = 24,

    PaymentCompleted = 30,
    PaymentFailed = 31,
    RefundIssued = 32,

    OrderCreated = 40,
    OrderCancelled = 41
}
