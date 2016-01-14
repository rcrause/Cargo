# Cargo.EntityFramework

This will allow you to use any of your EF 6 data context classes as a
database containing content items. This can then be used in conjuction with
`EntityFrameworkCargoDataSource` for use as a data source for Cargo.

## How to map

In your `DbContext` you need to add this line in the `OnModelCreating` method:

```C#
using Cargo;

protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    ...

    modelBuilder.MapCargoContent();

    ...
}
```

You can cause a prefix to be added to the tables:

```C#
modelBuilder.MapCargoContent(prefix: "Cargo_");
```

You can also specify a schema:

```C#
modelBuilder.MapCargoContent(schema: "cargo");
```

It is recommended that you do one or the other to avoid conflicts.

## How to use

To use a database as a data source for Cargo, you will have
to provide your own implementation of `CargoConfiguration` and
override the `GetDataSource` method:

```C#
public class MyCargoConfiguration : DefaultCargoConfiguration
{
    public override ICargoDataSource GetDataSource()
    {
        return new EntityFrameworkCargoDataSource(new MyDataContext());
    }
}
```

It may be prudent to use some kind of dependency resolution:

```C#
public override ICargoDataSource GetDataSource()
{
    if(DependencyResolver.Current != null)
    {
        return new EntityFrameworkCargoDataSource(DependencyResolver.Current.GetService<MyDataContext>());
    }
    else
    {
        return new EntityFrameworkCargoDataSource(new MyDataContext());
    }
}
```
