# Entity Framework Core State Tests
This project has a set of tests that demonstrate how EF Core Entity State works.  These examples were created to help me better understand how EF Core manages state, and what SQL it generates.  It came from a problem where the object model had the same object in the graph multiple times.  EF Core did fine if the graph was retrieved from the database, but if deserialized and serialized, it thinks I'm trying to track the same object twice.  These examples show that case.

# The Code
The project is was created using the .NET Core 2.2 NUnit template, with the addition of the `Microsoft.EntityFrameworkCore.SqlServer` and `Shouldly` packages.  It only consists of unit tests that test the various ways EF Core changes the state of objects.  I mainly used [VS Code](https://code.visualstudio.com/) on Windows for development.  There is also a sln file for use in Visual Studio 2019.  VS's [Live Unit Testing](https://docs.microsoft.com/en-us/visualstudio/test/live-unit-testing?view=vs-2019) feature works well with this project.

Like Microsoft's [EF Core samples](https://github.com/aspnet/EntityFramework.Docs), this code uses the code-first method, and a `(localdb)\mssqllocaldb` database server.

You can run the tests in VS Code or VS 2019 manually, or with VS 2019's Live Unit Testing.  To run the tests from the command line use:
```powershell
dotnet test
```
It will output the typical test results.  If you run tests from VSCode, it will also show the stdout logging that shows object states.  Details about the tests follow a brief discussion of state.

# EF Core Entity State
There are many good articles about EF Core (see links below for a few).  This blog will focus mainly on the State of objects and how they change within a `DbContext`.  This section gives a brief overview of State.

The `DbContext` tracks the state of objects with a `ChangeTracker` so that when `SaveChanges` is called, it knows what `UPDATE`, `INSERT`, or `DELETE` statements to generate.  The possible states are as follows:

|State     |Action on SaveChanges |Notes
|----------|----------------------|-----
|Added     |INSERT|PK can have no value if generated
|Modified  |UPDATE|Uses PK to update one or more columns
|Deleted   |DELETE|Uses PK to delete
|Unchanged |None|Retrieved from database, Attached, or after `SaveChanges`
|Detached  |None|Context is not tracking the object

> Note that EF Core also tracks changes to each property on an object with `IsModified` so on `UPDATE` it will only update changed data.  This article doesn't cover that aspect.

Objects that the context does not know about are in the `Detached` state.  For the context to know about an object, you either retrieve it from the database with the context or call one of the following methods on the context. (If you retrieve with `AsNoTracking` the objects will be `Detached`.)

|Method    |Resulting State
|----------|-----------------------
|Add       |Added
|Update    |Added if PK is not set else Modified. All properties will be 'modified'
|Find or retrieve via DbSet, etc.|Unchanged
|Attach    |Added if PK is not set else Unchanged

If your PKs are generated, `Update` will figure out if it's an adds or updates of the objects.  There may be cases where the object needs to have the PK set ahead of time, so you'll need to explicitly call `Add` to do an add.  For example, a child table's PK is a FK to the parent.

The following diagram shows how the EF Core State changed with various `DbContext` methods, or other actions.

![StateDiagram](/Doc/State.png)

# Test Details
The tests cover all the paths in the diagram in at least one flavor.

## The Model
A simple set of classes is used for the tests.  The `Thing` class is just a standalone (no relationships) class for testing basic state.

The `Loan` class has a `Lender` and `LenderContact` to complicate things the `LenderContact` also has a `Lender`.  This cyclic relationship was the impetus for writing this article so I could get a good understanding of how to solve that problem.

## ChangingThingStateShould.cs
This file tests state changes for one object with no relationships.  Each of the paths in the diagram are tested.

|Path                                    |Test                     |DML
|----------------------------------------|-------------------------|--
|Detached->Added->Unchanged              |Construct_Add_Save       |INSERT
|                                        |Construct_Update_Save    |INSERT
|                                        |Construct_Attach_Save    |INSERT
|                                        |Save_Find_AsNoTracking   |INSERT
|Detached->Modified->Unchanged           |Save_Update              |INSERT, UPDATE
|retrieve->Unchanged                     |Save_Find                |INSERT
|retrieve->Unchanged->Modified->Unchanged|Save_Find_Update         |INSERT, UPDATE
|retrieve->Unchanged->Deleted->Detached  |Save_Find_Delete         |INSERT, DELETE
|Detached->Unchanged                     |Attach_Saved             |INSERT
|                                        |Construct_Attach_SetState<sup>1</sup>|n/a
|                                        |Construct_SetState<sup>1</sup>       |n/a

<sup>1</sup>These explicitly set the state which is a path not shown in the diagram.

## ChangingGraphStatesShould.cs
This file does the basic actions on the `Loan` graph of objects.  When retrieving a `Loan` that includes the `Lender` and `LenderContact`, the `Lender` will be in the graph twice (since `LenderContact` also has the same `Lender`). Although it is in the graph twice, there is only one `Lender` instance. The tests assert that with `ReferenceEquals(foundLoan.Lender, foundLoan.LenderContact.Lender).ShouldBeTrue();`

When sending a `Loan` to a server (simulated with serialization in the tests) and trying to `Attach()` or `Update()` the object, EF will throw an exception saying that a `Lender` with the same Id is already being tracked.

Setting the `State` directly on the `Loan` avoids the recursive attaching of the other functions and then the `Loan` can be saved in the database.  There are other ways around this, but in my case, that's what was being sent to the server.  One way is to add `LenderId` and `LenderContactId` in the `Loan` object, which the `LoanEx` class does and `ChangingGraphExStatesShould.cs` demonstrates.

|Path                                       |Test                                   |SaveChanges Does
|-------------------------------------------|---------------------------------------|------------------
|Detached->Added->Unchanged                 |ConstructGraph_Attach_Save             |INSERT all 3
|                                           |ConstructGraph_Add_Save                |INSERT all 3
|                                           |ConstructGraph_Update_Save             |INSERT all 3
|Detached->Modified->Unchanged              |Save_Linq_Include_SetState_Graph       |UPDATE Loans
|retrieve->Unchanged                        |Save_Find_Graph                        |n/a
|                                           |Save_Linq_Graph                        |n/a
|                                           |Save_Linq_Include_Graph                |n/a
|Detached->error attaching                  |Save_Linq_Include_Attach_Graph_Throw   |n/a
|                                           |Save_Linq_Include_Update_Graph_Throw   |n/a
|retrieve->Unchanged->Modified->Unchanged   |Save_Find_Update_Graph                 |UPDATE Loans
|retrieve->Unchanged->Deleted->Detached     |ConstructGraph_Add_Save_Delete         |DELETE Loans
|Detached->Unchanged                        |ConstructGraph_Attach_SetState<sup>1</sup>|INSERT Lenders, LenderContacts<sup>2</sup>
|                                           |ConstructGraph_SetState<sup>1</sup>     |n/a
|                                           |Construct_WithExisting_Children_SetState<sup>1</sup>|INSERT Loans
|Detached->Unchanged->Modified              |Save_Linq_Include_TrackGraph_Func<sup>1</sup>      |n/a

<sup>1</sup>These explicitly set the state which is a path not shown in the diagram.<br>
<sup>2</sup>This doesn't save Loans as you may expect, but it explicitly sets the `Loan` state to `Unchanged`

The `Save_Linq_Include_TrackGraph_Func` test uses the `ChangeTracker.TrackGraph` method to hook into the change tracking process, which can be useful if you have a complex graph and want to control how the states are set.

# Final Thoughts
As with most things, the simple cases are easy enough, but you can often get into the weeds with complex cases.  In my case the documentation was not helpful when getting the error about tracking the object, so I ended up doing this empirical analysis of the issue.  I found chapter 9 of [Entity Framework Core in Action book.](https://www.manning.com/books/entity-framework-core-in-action) to have the best explantation of change tracking in EF Core.

# Links
* [Microsoft EF Core doc](https://docs.microsoft.com/en-us/ef/core/)
   * [Add](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.add?view=efcore-2.1)
   * [Attach](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.attach?view=efcore-2.1)
   * [Remove](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.remove?view=efcore-2.1)
   * [Update](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.update?view=efcore-2.1)
* [Microsoft EF Core doc on disconnected entities](https://docs.microsoft.com/en-us/ef/core/saving/disconnected-entities)
* [Microsoft EF Core doc on GitHub with tons of samples](https://github.com/aspnet/EntityFramework.Docs)
* [Entity Framework Core in Action book.](https://www.manning.com/books/entity-framework-core-in-action)  Chapter 9 has an extensive discussion on State
* [Entity Framework Tutorial article on disconnected entities](https://www.entityframeworktutorial.net/efcore/working-with-disconnected-entity-graph-ef-core.aspx)
* [Learning Entity Framework Core article on tracking](https://www.learnentityframeworkcore.com/dbcontext/change-tracker)