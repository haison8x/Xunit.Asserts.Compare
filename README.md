# Xunit.Asserts.Compare

xUnit deep comparing assertions by CompareNETObjects

NuGets
====
```
https://www.nuget.org/packages/xUnit.Asserts.Compare
```


Xunit.Asserts.Compare
==================
Couple of simple Xunit asserts that compare given objects using https://www.nuget.org/packages/CompareNETObjects/

- Compare objects
```csharp
DeepAssert.Equal(expected, actual);
```


- Ignore property
```csharp
DeepAssert.Equal(expected, actual, "ignoreProperty1", "ignoreProperty2");

```
