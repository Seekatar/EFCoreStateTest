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
        public void ConstructGraph_Update_Save()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo:true);
                loan.Log("Constructed", context);
                loan.Lender.ShouldNotBeNull();
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Update(loan);
                loan.Log("Update after constructed", context);
                context.LoanGraphStateShouldBe(loan, Added);

                context.SaveChanges(); // inserts into 3 tables
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
                context.LoanGraphStateShouldBe(loan, Unchanged);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }

        [Test]
        public void ConstructGraph_Add_Save()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            using (var context = new LoanContext())
            {
                var loan = new Loan(childrenToo: true);
                loan.Lender.ShouldNotBeNull();
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Add(loan);
                loan.Log("Add", context);
                context.LoanGraphStateShouldBe(loan, Added);

                context.SaveChanges(); // inserts into 3 tables
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }

        [Test]
        public void ConstructGraph_Attach_Save()
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

                context.SaveChanges(); // inserts into 3 tables
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }

        [Test]
        public void ConstructGraph_Attach_SetState()
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

                context.SaveChanges(); // inserts into 3 tables
                loan.Log("SaveChanges", context);
                context.LoanGraphStateShouldBe(loan, Unchanged);
                loan.Id.ShouldNotBe(Guid.Empty);
                loan.Lender.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Id.ShouldNotBe(Guid.Empty);
                loan.LenderContact.Lender.Id.ShouldBe(loan.Lender.Id);
            }
        }

        [Test]
        public void ConstructGraph_SetState()
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
        public void Save_Find_Graph()
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
        public void Save_Linq_Graph()
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
        public void Save_Linq_Include_Graph()
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
                ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();

                foundLoan.Log("Find", context);
                context.LoanGraphStateShouldBe(foundLoan, Unchanged);
                foundLoan.Lender.ShouldNotBeNull();
                foundLoan.LenderContact.ShouldNotBeNull();
            }
        }

        [Test]
        public void Save_Linq_Include_Attach_Graph_Throw()
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
        public void Save_Linq_Include_Update_Graph_Throw()
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
        public void Save_Linq_Include_TrackGraph_Func()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);
            var xferLoan = SaveNewLoanAndDetach();

            using (var context = new LoanContext())
            {
                context.ChangeTracker.TrackGraph(xferLoan, Unchanged, (node,state) => {
                    node.Entry.State = Unchanged;
                    if (node.Entry.Entity is Loan)
                    {
                        node.Entry.State = Modified;
                    }
                    return !(node.Entry.Entity is LenderContact);
                });
                xferLoan.Log("TrackGraph", context);
                context.StateShouldBe(xferLoan, Modified);
                context.StateShouldBe(xferLoan.Lender, Unchanged);
                context.StateShouldBe(xferLoan.LenderContact, Unchanged);
            }
        }

        [Test]
        public void Save_Linq_Include_SetState_Graph()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);

            var xferLoan = SaveNewLoanAndDetach();

            using (var context = new LoanContext())
            {
                context.Entry(xferLoan).State = Modified;
                xferLoan.Log("After set State on root", context);
                context.StateShouldBe(xferLoan, Modified);
                context.StateShouldBe(xferLoan.Lender, Detached);
                context.StateShouldBe(xferLoan.LenderContact, Detached);

                context.SaveChanges(); // updates only Loans table
                context.StateShouldBe(xferLoan, Unchanged);
                context.StateShouldBe(xferLoan.Lender, Detached);
                context.StateShouldBe(xferLoan.LenderContact, Detached);
            }
        }

        [Test]
        public void Construct_WithExisting_Children_SetState()
        {
            LogMsg(MethodBase.GetCurrentMethod().Name);

            var xferLoan = SaveNewLoanAndDetach();
            var loan = new Loan
            {
                Lender = xferLoan.Lender,
                LenderContact = xferLoan.LenderContact
            };

            using (var context = new LoanContext())
            {
                loan.Log("Constructed with existing children", context);
                context.LoanGraphStateShouldBe(loan, Detached);

                context.Entry(loan).State = Added;
                loan.Log("State Set", context);
                context.StateShouldBe(loan, Added);
                context.StateShouldBe(loan.Lender, Detached);
                context.StateShouldBe(loan.LenderContact, Detached);

                context.SaveChanges(); // inserts into only Loans table
                context.StateShouldBe(loan, Unchanged);
                context.StateShouldBe(loan.Lender, Detached);
                context.StateShouldBe(loan.LenderContact, Detached);
            }

            using (var context = new LoanContext())
            {
                var foundLoan = context.Set<Loan>()
                    .Include(o => o.Lender)
                    .Include(o => o.LenderContact)
                    .SingleOrDefault(o => o.Id == loan.Id);
                foundLoan.ShouldNotBeNull();
                foundLoan.Lender.ShouldNotBeNull();
                foundLoan.LenderContact.ShouldNotBeNull();
                foundLoan.Lender.Id.ShouldBe(loan.Lender.Id);
                foundLoan.LenderContact.Id.ShouldBe(loan.LenderContact.Id);
                ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();
            }
        }



        [Test]
        public void Save_Find_Update_Graph()
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

                context.SaveChanges(); // updates only Loans table
                context.StateShouldBe(foundLoan, Unchanged);
                foundLoan = context.Find<Loan>(loan.Id);
                foundLoan.Name.ShouldBe("Peoria");
            }
        }

        [Test]
        public void ConstructGraph_Add_Save_Delete()
        {
            var loan = SaveNewLoan();
            using (var context = new LoanContext())
            {
                var foundLoan = context.Set<Loan>()
                    .Include(o => o.Lender)
                    .Include(o => o.LenderContact)
                    .SingleOrDefault(o => o.Id == loan.Id);
                foundLoan.ShouldNotBeNull();
                foundLoan.Log("Find", context);
                context.StateShouldBe(foundLoan, Unchanged);
                ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();

                context.Remove(foundLoan);
                foundLoan.Log("Remove", context);
                context.StateShouldBe(foundLoan, Deleted);
                context.StateShouldBe(foundLoan.Lender, Unchanged); //  not cascading delete
                context.StateShouldBe(foundLoan.LenderContact, Unchanged);

                context.SaveChanges(); // deletes only from Loans
                foundLoan.Log("SaveChanged", context);
                context.StateShouldBe(foundLoan, Detached);
                context.StateShouldBe(foundLoan.Lender, Unchanged); //  not cascading delete
                context.StateShouldBe(foundLoan.LenderContact, Unchanged);
            }
        }

    }
}