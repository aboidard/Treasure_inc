
using System;

public class NotEnoughtMoneyException : Exception
{
    public NotEnoughtMoneyException()
    {}
    public NotEnoughtMoneyException(string message)
        : base(message)
    {}
}
