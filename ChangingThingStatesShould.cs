using Db;
using Model;
using NUnit.Framework;
using Extensions;
using Shouldly;
using static Microsoft.EntityFrameworkCore.EntityState;

namespace Tests
{
    internal class ChangingThingStatesShould : TestsBase
    {
        [Test]
        public void Construct_Add_Save_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var thing = new Thing();
                thing.Log("Constructed", context);
                context.StateShouldBe(thing, Detached);

                context.Add(thing);
                thing.Log("Updated", context);
                context.StateShouldBe(thing, Added);

                context.SaveChanges();
                thing.Log("SaveChanges", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Construct_Update_Save_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var thing = new Thing();
                thing.Log("Constructed", context);
                context.StateShouldBe(thing, Detached);

                context.Update(thing);
                thing.Log("Updated", context);
                context.StateShouldBe(thing, Added);

                context.SaveChanges();
                thing.Log("SaveChanges", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Construct_Attach_Save_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var thing = new Thing();
                thing.Log("Constructed", context);
                context.StateShouldBe(thing, Detached);

                context.Attach(thing);
                thing.Log("Attached", context);
                context.StateShouldBe(thing, Added);

                context.SaveChanges();
                thing.Log("SaveChanges", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Attach_Saved_SingleObject()
        {
            var thing = SaveNewThing();
            using (var context = new LoanContext())
            {
                context.StateShouldBe(thing, Detached);
                context.Attach(thing);
                thing.Log("Attached a saved", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Construct_Attach_SetState_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var thing = new Thing();
                thing.Log("Constructed", context);
                context.StateShouldBe(thing, Detached);

                context.Attach(thing).State = Unchanged;
                thing.Log("Attached", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Construct_SetState_SingleObject()
        {
            using (var context = new LoanContext())
            {
                var thing = new Thing();
                thing.Log("Constructed", context);
                context.StateShouldBe(thing, Detached);

                context.Entry(thing).State = Unchanged;
                thing.Log("Attached", context);
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Save_Find_SingleObject()
        {
            var thing = SaveNewThing();
            using (var context = new LoanContext())
            {
                var foundThing = context.Find<Thing>(thing.Id);
                foundThing.ShouldNotBeNull();
                foundThing.Log("Find", context);
                context.StateShouldBe(foundThing, Unchanged);
            }
        }

        [Test]
        public void Save_Update_SingleObject()
        {
            var thing = SaveNewThing();
            using (var context = new LoanContext())
            {
                context.StateShouldBe(thing, Detached);
                context.Update(thing);
                context.StateShouldBe(thing, Modified);

                context.SaveChanges(); // will update db even no real changes
                context.StateShouldBe(thing, Unchanged);
            }
        }

        [Test]
        public void Save_Find_Update_SingleObject()
        {
            var thing = SaveNewThing();
            using (var context = new LoanContext())
            {
                var foundThing = context.Find<Thing>(thing.Id);
                foundThing.ShouldNotBeNull();
                foundThing.Log("Find", context);
                context.StateShouldBe(foundThing, Unchanged);

                foundThing.Name = "Peoria";
                context.StateShouldBe(foundThing, Modified);

                context.SaveChanges();
                context.StateShouldBe(foundThing, Unchanged);
            }
        }

        [Test]
        public void Save_Find_Delete_SingleObject()
        {
            var thing = SaveNewThing();
            using (var context = new LoanContext())
            {
                var foundThing = context.Find<Thing>(thing.Id);
                foundThing.ShouldNotBeNull();
                foundThing.Log("Find", context);
                context.StateShouldBe(foundThing, Unchanged);

                context.Remove(foundThing);
                context.StateShouldBe(foundThing, Deleted);

                context.SaveChanges();
                context.StateShouldBe(foundThing, Detached);
            }
        }
    }
}