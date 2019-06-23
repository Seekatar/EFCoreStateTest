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

        protected Address SaveNewAddress()
        {
            using ( var context = new LoanContext())
            {
                var address = new Address();
                context.Update(address);
                context.StateShouldBe(address,Added);
                context.SaveChanges();
                context.StateShouldBe(address,Unchanged);
                return address;
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
                object.ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();

                xferLoan = JsonConvert.DeserializeObject<Loan>(JsonConvert.SerializeObject(foundLoan));
                context.LoanGraphStateShouldBe(xferLoan, Detached);
                xferLoan.Log("After Deserialize", context);
            }

            return xferLoan;
        }

        protected Address SaveNewAddressAndDetach()
        {
            var address = SaveNewAddress();

            Address xferAddress = null;

            using (var context = new LoanContext())
            {
                var foundAddress = context.Set<Address>()
                            .SingleOrDefault(o => o.Id == address.Id);
                foundAddress.ShouldNotBeNull();
                foundAddress.Log("Find", context);
                context.StateShouldBe(foundAddress, Unchanged);

                xferAddress = JsonConvert.DeserializeObject<Address>(JsonConvert.SerializeObject(foundAddress));
                context.StateShouldBe(xferAddress, Detached);
                xferAddress.Log("After Deserialize", context);
            }

            return xferAddress;
        }

    }
}