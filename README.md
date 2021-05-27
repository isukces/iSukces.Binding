# iSukces.Binding
This package is an implementation of data binding inspired by WPF data binding. I had to port WPF application into Windows Forms platform and I found that I need a framework for easy data binding in various scenarios.

## Deep binding
Library supports binding for properties in any level like. ```Source.DataModel.SomeObject.SomeProperty```. Changes at each level ale checked.

## Disposing
Bindings are managed by ```BindingManager``` which supports ```IDisposable``` interface. 
Disposing manager releases all bindings and unsubscribes property changing events.
It is also possible to dispose single binding.

## Listener scenario
Listener scenario is the simplest way to use binding. It's necessary to choose source object, binding path and listener action. Each time the property value is changed the given action is invoked.

#### Example
```c#
// create binding manager i.e. one for visual component like window or user control
var bm  = new BindingManager();

IDisposable binding = bm.From(this)
    .WithPath("ViewModel.Title")
    .CreateListener((value, kind) =>
      {
         // see table below
          ...              
      });

// at the end i.e. closing window dispose binding or BindingManager
binding.Dispose(); // disposes single binding
// or
bm.Dispose(); // disposes all bindings
```


| kind                                     | condition                                                                       |                    value                   |
|------------------------------------------|---------------------------------------------------------------------------------|--------------------------------------------|
| ```StartBinding``` or ```ValueChanged``` | source is not null and property exists and is readable                          | property value                             |
| ```StartBinding``` or ```ValueChanged``` | source is not null and property exists and property reading throws an exception | ```BindingSpecial.PropertyReadException``` |
| ```StartBinding``` or ```ValueChanged``` | source is null or property doesn't exist                                        | ```BindingSpecial.NotSet```                |
| ```EndBinding```                         |                                                                                 | ```BindingSpecial.Unbound```               |
