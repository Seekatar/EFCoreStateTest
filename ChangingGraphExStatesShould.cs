using Db;
using Model;
using NUnit.Framework;
using Extensions;
using Shouldly;
using static Microsoft.EntityFrameworkCore.EntityState;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Reflection;
using static Extensions.LogExts;

namespace Tests
{
    internal class ChangingGraphExStatesShould : TestsBase
    {
        [Test]
        public void ConstructGraphEx_Update_Save()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var loan = SaveNewLoanEx();

            using (var context = new LoanContext())
            {
                var foundLoan = context.Find<LoanEx>(loan.Id);
                foundLoan.ShouldNotBeNull();
                foundLoan.LenderContactId.ShouldNotBeNull();
                foundLoan.LenderId.ShouldNotBeNull();
                foundLoan.LenderContact.ShouldBeNull();
                foundLoan.Lender.ShouldBeNull();
            }
        }

        [Test]
        public void Save_Linq_Include_Attach_Graph()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var xferLoan = SaveNewLoanExAndDetach();

            using (var context = new LoanContext())
            {
                // in case of Loan with childrenToo, this throws since Lender in graph twice
                context.Attach(xferLoan);
                context.StateShouldBe(xferLoan, Unchanged);
            }
        }

        [Test]
        public void Save_Linq_Include_Update_Graph()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var xferLoan = SaveNewLoanExAndDetach();

            using (var context = new LoanContext())
            {
                // in case of Loan with childrenToo, this throws since Lender in graph twice
                context.Update(xferLoan);
                context.StateShouldBe(xferLoan, Modified);
            }
        }


    }
}