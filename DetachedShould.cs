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
        public void Attach_Thing()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var thing = SaveNewThingAndDetach();

            using ( var context = new LoanContext())
            {
                context.Attach(thing);
                context.StateShouldBe(thing,Unchanged);

                thing.Name = "Peoria";
                context.StateShouldBe(thing,Modified);
                context.SaveChanges();
            }
            using ( var context = new LoanContext())
            {
                thing = context.Set<Thing>().Find(thing.Id);
                thing.ShouldNotBeNull();
                thing.Name.ShouldBe("Peoria");
            }
        }

        [Test]
        public void Update_Thing()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var thing = SaveNewThingAndDetach();

            using ( var context = new LoanContext())
            {
                context.Update(thing);
                context.StateShouldBe(thing,Modified);

                thing.Name = "Peoria";
                context.StateShouldBe(thing,Modified);
                context.SaveChanges();
            }
            using ( var context = new LoanContext())
            {
                thing = context.Set<Thing>().Find(thing.Id);
                thing.ShouldNotBeNull();
                thing.Name.ShouldBe("Peoria");
            }
        }

        [Test]
        public void Throw_Add_Duplicate_Thing()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var thing = SaveNewThingAndDetach();

            using ( var context = new LoanContext())
            {
                context.Add(thing);
                context.StateShouldBe(thing,Added);

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