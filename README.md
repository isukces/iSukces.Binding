
# iSukces.Binding
This package is an implementation of data binding inspired by WPF data binding. I had to port WPF application into Windows Forms platform and I found that I need a framework for easy data binding in various scenarios.

__Content__
* [Deep binding](#deep-binding)
* [Disposing](#disposing)
* [Listener scenario](#listener-scenario)
    * [Listener scenario with converter](#listener-scenario-with-value-converter)
    * [Listener dispatcher](#listener-dispatcher)


## Deep binding
Library supports binding for properties in any level like. ```Source.DataModel.SomeObject.SomeProperty```. Changes at each level are checked.


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
 .WithPath("ViewModel.Title") .CreateListener(info => { // see table below ...      });  
  
// at the end i.e. closing window dispose binding or BindingManager  
binding.Dispose(); // disposes single binding  
// or  
bm.Dispose(); // disposes all bindings  
```  

```IValueInfo``` object passed into lister method provides binding value, updating king (see table below) and last valid source value if current value is not valid.

#### Listener kinds

Helps to identify value source:

| kind               | meaning                                   |  
|--------------------|-------------------------------------------|  
| ```StartBinding``` | ust after binding                         |  
| ```EndBinding```   | binding was disposed                      |  
| ```UpdateSource``` | source was changed as a result of binding |  
| ```ValueChanged``` | source was changed                        |  

#### Listener rules

| kind                                     | condition                                                                       |                    value                   |  
|------------------------------------------|---------------------------------------------------------------------------------|--------------------------------------------|  
| not ```EndBinding```                     | source is not null and property exists and is readable                          | property value                             |  
| not ```EndBinding```                     | source is not null and property exists and property reading throws an exception | ```BindingSpecial.PropertyReadException``` |  
| not ```EndBinding```                     | source is null or property doesn't exist                                        | ```BindingSpecial.NotSet```                |  
| ```EndBinding```                         |                                                                                 | ```BindingSpecial.Unbound```               |  


## #Listener scenario with value converter

Similarly to WPF data binding, it's possible to put a value converter between data source and listener so type conversion can be performed automatically. In the listener scenario, one-way conversion is used. It's possible to use two-way conversion in more advanced scenarios.

#### Example
```c#  
var model = new Model();  
var builder = bm.From(model, q=>q.DecimalNumber);  
builder.Converter   = NumberValueConverter.Instance;  
builder.CultureInfo = CultureInfo.GetCultureInfo("en-GB");  
builder.ConverterParameter = "c2";  
var binding =builder.CreateListener(info =>  
{  
 Console.WriteLine(info.Value);});  
```  
Once we set ```model.DecimalValue``` with value ```99.45```, the text ```£99.45``` will be passed into listener due to decimal value conversion with ```C2``` format and ```gb-GB``` culture info.



## Listener dispatcher
It is also possible to specify a dispatcher to be used when listener action is invoked.

#### Example
```c#  
var model = new Model();  
var builder = bm.From(model, q=>q.DecimalNumber);  
builder.ListenerDispatcher = Dispatcher.CurrentDispatcher;  
var binding = builder.CreateListener(info =>  
{  
 // this method is invoked with d});  
```
Note that data conversion, if any, is always performed in data thread.