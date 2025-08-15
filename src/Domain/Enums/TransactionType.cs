namespace Domain.Enums;

public enum TransactionType
{
    Deposit,       // D
    Withdrawal,    // W
    Interest       // I (credited month-end; TxnId left blank per spec)
}
