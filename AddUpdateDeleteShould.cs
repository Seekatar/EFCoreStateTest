using Db;
using Model;
using NUnit.Framework;
using Extensions;
using Shouldly;
using System;

namespace Tests
{
    internal class AddUpdateDeleteShould : TestsBase
    {
        [Test]
        public void Construct_Save()
        {
            using ( var context = new LoanContext())
            {
                var loan = new Loan();
                context.SaveChanges();
                loan.Log("SaveChanges", context);
                loan.Id.ShouldBe(Guid.Empty);
            }
        }

        [Test]
        public void Construct_Add_SaveRoot()
        {
            using ( var context = new LoanContext())
            {
                var loan = new Loan();
                context.Add(loan);
                loan.Log("Add", context);
                context.SaveChanges();
                loan.Log("SaveChanges", context);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.ShouldBeNull();
            }
        }

        [Test]
        public void Construct_Add_SaveGraph()
        {
            using ( var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Lender.ShouldNotBeNull();
                context.Add(loan);
                loan.Log("Add", context);
                context.SaveChanges();
                loan.Log("SaveChanges", context);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }

        [Test]
        public void Construct_Update_SaveGraph()
        {
            using ( var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                loan.Lender.ShouldNotBeNull();
                context.Update(loan);
                loan.Log("Add", context);
                context.SaveChanges();
                loan.Log("SaveChanges", context);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }
    }
}