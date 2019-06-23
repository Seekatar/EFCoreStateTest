using Microsoft.EntityFrameworkCore;
using Model;
using Shouldly;
using static System.Console;

namespace Extensions
{
    public static class LogExts
    {
        public static void Log(this Loan loan, string msg, DbContext context)
        {
            if (loan != null)
            {
                WriteLine($"\r\nLoan '{msg}' -- state = {context.Entry(loan).State} -- id = {loan.Id}");
                loan?.Lender.Log(msg, context);
                loan?.LenderContact.Log(msg, context);
            }
            else
                WriteLine($"\r\nLoan '{msg}' -- null");
        }
        public static void Log(this Lender lender, string msg, DbContext context, string indent = "")
        {
            if (lender != null)
            {
                WriteLine($"{indent}    Lender '{msg}' -- state = {context.Entry(lender).State} -- id = {lender.Id}");
            }
            else
                WriteLine($"{indent}    Lender '{msg}' -- null");
        }
        public static void Log(this LenderContact contact, string msg, DbContext context)
        {
            if (contact != null)
            {
                WriteLine($"    LenderContact '{msg}' -- state = {context.Entry(contact).State} -- id = {contact.Id}");
                contact?.Lender.Log(msg, context, "    ");
            }
            else
                WriteLine($"    LenderContact '{msg}' -- null");
        }
        public static void Log(this Address address, string msg, DbContext context)
        {
            if (address != null)
            {
                WriteLine($"\r\nAddress '{msg}' -- state = {context.Entry(address).State} -- id = {address.Id}");
            }
            else
                WriteLine($"\r\nAddress '{msg}' -- null");
        }
        public static void StateShouldBe(this DbContext context, object obj, Microsoft.EntityFrameworkCore.EntityState state)
        {
            context.Entry(obj).State.ShouldBe(state);
        }
        public static void LoanGraphStateShouldBe(this DbContext context, Loan loan, Microsoft.EntityFrameworkCore.EntityState state)
        {
            context.StateShouldBe(loan, state);
            context.StateShouldBe(loan.Lender, state);
            context.StateShouldBe(loan.LenderContact, state);
        }
        public static void LogMsg(string msg)
        {
            WriteLine($"\r\n---- {msg}");
        }
    }
}