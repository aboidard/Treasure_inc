
using System;

public class TooMuchExpeditionsException : Exception
{
    public TooMuchExpeditionsException()
    { }
    public TooMuchExpeditionsException(string message)
        : base(message)
    { }
}
