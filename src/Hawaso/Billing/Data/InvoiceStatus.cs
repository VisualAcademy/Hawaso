namespace Hawaso.Billing.Data;

public enum InvoiceStatus
{
    Draft,
    Issued,
    Sent,
    Paid,
    Void,
    PaymentProcessing = 5
}
