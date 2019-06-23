using Db;
using Model;
using NUnit.Framework;
using Extensions;
using Shouldly;
using static Microsoft.EntityFrameworkCore.EntityState;

namespace Tests
{
    internal class ChangingAddressStatesShould : TestsBase
    {
        [Test]
        public void Construct_Update_Save_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var address = new Address();
                address.Log("Constructed", context);
                context.StateShouldBe(address, Detached);

                context.Update(address);
                address.Log("Updated", context);
                context.StateShouldBe(address, Added);

                context.SaveChanges();
                address.Log("SaveChanges", context);
                context.StateShouldBe(address, Unchanged);
            }
        }

        [Test]
        public void Construct_Add_Save_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var address = new Address();
                address.Log("Constructed", context);
                context.StateShouldBe(address, Detached);

                context.Add(address);
                address.Log("Updated", context);
                context.StateShouldBe(address, Added);

                context.SaveChanges();
                address.Log("SaveChanges", context);
                context.StateShouldBe(address, Unchanged);
            }
        }

        [Test]
        public void Construct_Attach_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var address = new Address();
                address.Log("Constructed", context);
                context.StateShouldBe(address, Detached);

                context.Attach(address);
                address.Log("Attached", context);
                context.StateShouldBe(address, Added);
            }
        }

        [Test]
        public void Attach_Saved_SingleObject()
        {
            var address = SaveNewAddress();
            using (var context = new LoanContext())
            {
                context.StateShouldBe(address, Detached);
                context.Attach(address);
                address.Log("Attached a saved", context);
                context.StateShouldBe(address, Unchanged);
            }
        }

        [Test]
        public void Construct_Attach_SetState_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var address = new Address();
                address.Log("Constructed", context);
                context.StateShouldBe(address, Detached);

                context.Attach(address).State = Unchanged;
                address.Log("Attached", context);
                context.StateShouldBe(address, Unchanged);
            }
        }

        [Test]
        public void Construct_SetState_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var address = new Address();
                address.Log("Constructed", context);
                context.StateShouldBe(address, Detached);

                context.Entry(address).State = Unchanged;
                address.Log("Attached", context);
                context.StateShouldBe(address, Unchanged);
            }
        }

        [Test]
        public void Save_Find_SingleObject()
        {
            var address = SaveNewAddress();
            using (var context = new LoanContext())
            {
                var foundAddress = context.Find<Address>(address.Id);
                foundAddress.ShouldNotBeNull();
                foundAddress.Log("Find", context);
                context.StateShouldBe(foundAddress, Unchanged);
            }
        }

        [Test]
        public void Save_Find_Update_SingleObject()
        {
            var address = SaveNewAddress();
            using (var context = new LoanContext())
            {
                var foundAddress = context.Find<Address>(address.Id);
                foundAddress.ShouldNotBeNull();
                foundAddress.Log("Find", context);
                context.StateShouldBe(foundAddress, Unchanged);

                foundAddress.City = "Peoria";
                context.StateShouldBe(foundAddress, Modified);

                context.SaveChanges();
                context.StateShouldBe(foundAddress, Unchanged);
            }
        }

        [Test]
        public void Save_Find_Delete_SingleObject()
        {
            var address = SaveNewAddress();
            using (var context = new LoanContext())
            {
                var foundAddress = context.Find<Address>(address.Id);
                foundAddress.ShouldNotBeNull();
                foundAddress.Log("Find", context);
                context.StateShouldBe(foundAddress, Unchanged);

                context.Remove(foundAddress);
                context.StateShouldBe(foundAddress, Deleted);

                context.SaveChanges();
                context.StateShouldBe(foundAddress, Detached);
            }
        }
    }
}