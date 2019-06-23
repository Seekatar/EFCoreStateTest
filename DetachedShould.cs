using Db;
using Model;
using NUnit.Framework;
using Extensions;
using Shouldly;
using static Microsoft.EntityFrameworkCore.EntityState;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static Extensions.LogExts;

namespace Tests
{
    internal class DetachedShould : TestsBase
    {
        [Test]
        public void Attach_Address()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var address = SaveNewAddressAndDetach();

            using ( var context = new LoanContext())
            {
                context.Attach(address);
                context.StateShouldBe(address,Unchanged);

                address.City = "Peoria";
                context.StateShouldBe(address,Modified);
                context.SaveChanges();
            }
            using ( var context = new LoanContext())
            {
                address = context.Set<Address>().Find(address.Id);
                address.ShouldNotBeNull();
                address.City.ShouldBe("Peoria");
            }
        }

        [Test]
        public void Update_Address()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var address = SaveNewAddressAndDetach();

            using ( var context = new LoanContext())
            {
                context.Update(address);
                context.StateShouldBe(address,Modified);

                address.City = "Peoria";
                context.StateShouldBe(address,Modified);
                context.SaveChanges();
            }
            using ( var context = new LoanContext())
            {
                address = context.Set<Address>().Find(address.Id);
                address.ShouldNotBeNull();
                address.City.ShouldBe("Peoria");
            }
        }

        [Test]
        public void Throw_Add_Duplicate_Address()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var address = SaveNewAddressAndDetach();

            using ( var context = new LoanContext())
            {
                context.Add(address);
                context.StateShouldBe(address,Added);

                try
                {
                    context.SaveChanges();
                    true.ShouldBeFalse();
                }
                catch (DbUpdateException)
                {
                    true.ShouldBeTrue(); // duplicate key since already there.
                }

            }
        }
    }
}