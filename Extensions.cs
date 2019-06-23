using System;
using Microsoft.EntityFrameworkCore;
using Model;
using Shouldly;
using static System.Console;

namespace Extensions
{
    internal static class LogExts
    {
        private static bool _shouldLog = true;
        public static bool ShouldLog { get {return  _shouldLog; } set {_shouldLog = true;}}

        private static void WriteLog(string msg)
        {
            if ( _shouldLog )
                WriteLine(msg);
        }

        public static void Log(this LoanBase loan, string msg, DbContext context)
        {
            if (loan != null)
            {
                WriteLog($"{Environment.NewLine}Loan '{msg}' -- state = {context.Entry(loan).State} -- id = {loan.Id}");
                loan?.Lender.Log(msg, context);
                loan?.LenderContact.Log(msg, context);
            }
            else
                WriteLog($"{Environment.NewLine}Loan '{msg}' -- null");
        }
        public static void Log(this Lender lender, string msg, DbContext context, string indent = "")
        {
            if (lender != null)
            {
                WriteLog($"{indent}    Lender '{msg}' -- state = {context.Entry(lender).State} -- id = {lender.Id}");
            }
            else
                WriteLog($"{indent}    Lender '{msg}' -- null");
        }
        public static void Log(this LenderContact contact, string msg, DbContext context)
        {
            if (contact != null)
            {
                WriteLog($"    LenderContact '{msg}' -- state = {context.Entry(contact).State} -- id = {contact.Id}");
                contact?.Lender.Log(msg, context, "    ");
            }
            else
                WriteLog($"    LenderContact '{msg}' -- null");
        }
        public static void Log(this Thing thing, string msg, DbContext context)
        {
            if (thing != null)
            {
                WriteLog($"{Environment.NewLine}Thing '{msg}' -- state = {context.Entry(thing).State} -- id = {thing.Id}");
            }
            else
                WriteLog($"{Environment.NewLine}Thing '{msg}' -- null");
        }
        public static void StateShouldBe(this DbContext context, object obj, Microsoft.EntityFrameworkCore.EntityState state)
        {
            context.Entry(obj).State.ShouldBe(state);
        }
        public static void LoanGraphStateShouldBe(this DbContext context, LoanBase loan, Microsoft.EntityFrameworkCore.EntityState state)
        {
            context.StateShouldBe(loan, state);
            context.StateShouldBe(loan.Lender, state);
            context.StateShouldBe(loan.LenderContact, state);
        }
        public static void LogMsg(string msg)
        {
            WriteLog($"{Environment.NewLine}---- {msg}");
        }
    }
}