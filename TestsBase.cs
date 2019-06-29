using Db;
using Model;
using NUnit.Framework;
using Extensions;
using static Microsoft.EntityFrameworkCore.EntityState;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Newtonsoft.Json;
using System.Linq;

namespace Tests
{
    internal class TestsBase {
        [SetUp]
        public void Setup()
        {
            using (var context = new LoanContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }

        protected Thing SaveNewThing()
        {
            using ( var context = new LoanContext())
            {
                var thing = new Thing();
                context.Update(thing);
                context.StateShouldBe(thing,Added);
                context.SaveChanges();
                context.StateShouldBe(thing,Unchanged);
                return thing;
            }
        }

        protected Loan SaveNewLoan()
        {
            using ( var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                context.Update(loan);
                context.StateShouldBe(loan,Added);
                context.StateShouldBe(loan.Lender,Added);
                context.StateShouldBe(loan.LenderContact,Added);
                context.SaveChanges();
                context.StateShouldBe(loan,Unchanged);
                context.StateShouldBe(loan.Lender,Unchanged);
                context.StateShouldBe(loan.LenderContact,Unchanged);
                return loan;
            }
        }
        protected LoanEx SaveNewLoanEx()
        {
            using ( var context = new LoanContext())
            {
                var loan = new LoanEx(childrenToo:true);
                context.Update(loan);
                context.StateShouldBe(loan,Added);
                context.StateShouldBe(loan.Lender,Added);
                context.StateShouldBe(loan.LenderContact,Added);
                context.SaveChanges();
                context.StateShouldBe(loan,Unchanged);
                context.StateShouldBe(loan.Lender,Unchanged);
                context.StateShouldBe(loan.LenderContact,Unchanged);
                return loan;
            }
        }

        protected Loan SaveNewLoanAndDetach()
        {
            var loan = SaveNewLoan();
            Loan xferLoan = null;
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
                ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();

                xferLoan = JsonConvert.DeserializeObject<Loan>(JsonConvert.SerializeObject(foundLoan));
                context.LoanGraphStateShouldBe(xferLoan, Detached);
                xferLoan.Log("After Deserialize", context);
            }

            return xferLoan;
        }


        protected LoanEx SaveNewLoanExAndDetach()
        {
            var loan = SaveNewLoanEx();
            LoanEx xferLoan = null;
            using (var context = new LoanContext())
            {
                var foundLoan = context.Set<LoanEx>()
                            .SingleOrDefault(o => o.Id == loan.Id);
                foundLoan.ShouldNotBeNull();
                foundLoan.Log("Find", context);
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan.LenderId.ShouldNotBeNull();
                foundLoan.LenderContactId.ShouldNotBeNull();

                xferLoan = JsonConvert.DeserializeObject<LoanEx>(JsonConvert.SerializeObject(foundLoan));
                context.StateShouldBe(xferLoan, Detached);
                xferLoan.Log("After Deserialize", context);
            }

            return xferLoan;
        }

        protected Thing SaveNewThingAndDetach()
        {
            var thing = SaveNewThing();

            Thing xferThing = null;

            using (var context = new LoanContext())
            {
                var foundThing = context.Set<Thing>()
                            .SingleOrDefault(o => o.Id == thing.Id);
                foundThing.ShouldNotBeNull();
                foundThing.Log("Find", context);
                context.StateShouldBe(foundThing, Unchanged);

                xferThing = JsonConvert.DeserializeObject<Thing>(JsonConvert.SerializeObject(foundThing));
                context.StateShouldBe(xferThing, Detached);
                xferThing.Log("After Deserialize", context);
            }

            return xferThing;
        }

    }
}