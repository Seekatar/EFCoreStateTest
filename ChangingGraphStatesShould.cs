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
    internal class ChangingGraphStatesShould : TestsBase
    {
        [Test]
        public void Construct_Update_Save_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Update(loan);
                loan.Log("Update after constructed", context);
                context.LoanGraphStateShouldBe(loan, Added);

                context.SaveChanges();
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
            }
        }

        [Test]
        public void Construct_Add_Save_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Add(loan);
                loan.Log("Add", context);
                context.LoanGraphStateShouldBe(loan, Added);

                context.SaveChanges();
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
            }
        }

        [Test]
        public void Construct_Attach_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Attach(loan);
                loan.Log("Attach after constructed", context);
                context.LoanGraphStateShouldBe(loan, Added);
            }
        }

        [Test]
        public void Construct_Attach_SetState_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Attach(loan).State = Unchanged; // Attach set graph to Added
                loan.Log("Attach and State set", context);
                context.StateShouldBe(loan, Unchanged);
                context.StateShouldBe(loan.Lender, Added);
                context.StateShouldBe(loan.LenderContact, Added);
            }
        }

        [Test]
        public void Construct_SetState_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Entry(loan).State = Unchanged;
                loan.Log("State Set", context);
                context.StateShouldBe(loan, Unchanged);
                context.StateShouldBe(loan.Lender, Detached);
                context.StateShouldBe(loan.LenderContact, Detached);
            }
        }

        [Test]
        public void Save_Find_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var loan = SaveNewLoan();
            using (var context = new LoanContext())
            {
                var foundLoan = context.Find<Loan>(loan.Id);
                foundLoan.ShouldNotBeNull();

                foundLoan.Log("Find", context);
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan.Lender.ShouldBeNull();
                foundLoan.LenderContact.ShouldBeNull();
            }
        }

        [Test]
        public void Save_Linq_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var loan = SaveNewLoan();
            using (var context = new LoanContext())
            {
                var foundLoan = context.Set<Loan>()
                            .SingleOrDefault(o => o.Id == loan.Id);
                foundLoan.ShouldNotBeNull();

                foundLoan.Log("Find", context);
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan.Lender.ShouldBeNull();
                foundLoan.LenderContact.ShouldBeNull();
            }
        }

        [Test]
        public void Save_Linq_Include_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var loan = SaveNewLoan();
            using (var context = new LoanContext())
            {
                var foundLoan = context.Set<Loan>()
                            .Include(o => o.Lender)
                            .Include(o => o.LenderContact)
                            .SingleOrDefault(o => o.Id == loan.Id);
                foundLoan.ShouldNotBeNull();

                foundLoan.Log("Find", context);
                context.LoanGraphStateShouldBe(foundLoan, Unchanged);
                foundLoan.Lender.ShouldNotBeNull();
                foundLoan.LenderContact.ShouldNotBeNull();
            }
        }

        [Test]
        public void Throws_Save_Linq_Include_Attach_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var xferLoan = SaveNewLoanAndDetach();

            using (var context = new LoanContext())
            {
                try
                {
                    // throws since Attach in recursive and Lender is in graph twice
                    context.Attach(xferLoan);
                    true.ShouldBeFalse();
                }
                catch (InvalidOperationException) {}
            }
        }

        [Test]
        public void Throws_Save_Linq_Include_Update_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var xferLoan = SaveNewLoanAndDetach();

            using (var context = new LoanContext())
            {
                try
                {
                    // throws since Attach in recursive and Lender is in graph twice
                    context.Update(xferLoan);
                    true.ShouldBeFalse();
                }
                catch (InvalidOperationException) {}
            }
        }

        [Test]
        public void Save_Linq_Include_Attach_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);

            // can't just switch context, just do
            // something like this to make new objects
            var xferLoan = SaveNewLoanAndDetach();

            using (var context = new LoanContext())
            {
                context.Entry(xferLoan).State = Modified;
                xferLoan.Log("After set State on root", context);
                context.StateShouldBe(xferLoan, Modified);
                context.StateShouldBe(xferLoan.Lender, Detached);
                context.StateShouldBe(xferLoan.LenderContact, Detached);
            }
        }

        [Test]
        public void Save_Find_Update_GraphObject()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var loan = SaveNewLoan();
            using (var context = new LoanContext())
            {
                var foundLoan = context.Find<Loan>(loan.Id);
                foundLoan.ShouldNotBeNull();
                foundLoan.Log("Find", context);
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan.Name = "Peoria";
                context.StateShouldBe(foundLoan, Modified);
                context.SaveChanges();
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan = context.Find<Loan>(loan.Id);
                foundLoan.Name.ShouldBe("Peoria");
            }
        }
    }
}