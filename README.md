# MonolithicExtensions

**Original Author:** Carlos Sanchez, 2018

## What is this?

MonolithicExtensions is a general extension library for all the random bits
of code I wrote which I felt could be reused. It creates four dlls with varying
levels of portability. 

There's "Portable" for anything that runs Xamarin, "General" for anything that 
runs Mono and is most likely a standard computer operating system 
(ie Windows/Linux/etc.), "Windows" for anything ABSOLUTELY specific to windows 
(like GAC registration), and "Android" for Android specific things.

Most of the functions have unit tests and they all passed when I last 
worked on the project, so use those as a baseline if anything breaks.

MonolithicExtensions follows a "global stateless function" ideology (most of 
the time). This means the functionality provided is expected to be simple 
and not change, and certainly not have side effects. Instead of creating 
interfaced services so functionality can be swapped out, I figured the 
tasks being performed (a) would never need to be changed and (b) not have
any dependencies, so I did not use a complex model and instead settled
for static classes with static, stateless functions.

## Things to note

I threw a lot of this stuff together and the naming conventions are inconsistent.
Do not rely on the fact that some things are called "Service" and others 
"Extensions" to know what the code does. However, most of the functions should
have summaries, so you should at least get intellisense to give a picture of
what anything does.

## Main Functionality

**DIFactory:** An in-house implementation of a dependency-injection container.
 It provides methods for object creation and disposal, and has methods for
 profilerating global settings down through objects. It also supports 
 merging of factories so you can have various "baseline" factories each for
 their own project, then merge all the required factories together into one
 for your given service/executable.

**ILogger/Logging:** There are many logging systems across all the platforms
 .NET runs on. They all BASICALLY work the same though, so they can all be 
 wrapped to provide a common interface for classes to perform logging.
 This is the ILogger interface and the MonolithicExtensions logging system 
 in general. There are implementations of the ILogger interface for 
 log4net (used to log to files on Windows) and for Android's 
 logging system. 

 In most of my classes, you'll see an "ILogger" object and code in the 
 constructor to initialize the ILogger. They all create a proxy logger
 from a "default" logger, which is configured to wrap a real logging
 service based on how the logger is configured. Examples of configuration 
 and logger usage can be seen in the Service1.cs file of 
 MonolithicExtensions.TestService.

**SerializationExtensions:** The serialization services provided by
 .NET are great for simple objects, but not for complex ones. The default
 settings for Newtonsoft's json library aren't setup for this either,
 so SerializationExtensions creates functions that wrap Newtonsoft json
 serialization functions in order to serialize most objects for 
 transport perfectly, including private/protected fields. If your 
 use case only requires public fields (which many do) and not a perfect
 object recreation, you can use the default Newtonsoft json serialization
 and ignore these functions.

**RemoteCallSystem:** WCF made things more complicated, not less, especially
 when dependency injection was involved. The remote call system implemented 
 in MonolithicExtensions is a simplified RPC system whose baseline implementation
 communicates over a lightweight Http server using JSON for serialization.
 You can make calls against a remote object's public methods, although it's a
 little more work on the client side in order to setup these calls.

 If an in-house solution is not desirable, WCF is perfectly fine. However, this
 RPC system is being used in the IDNUpdate service, so it might be worth it to
 get familiar with it (or replace it with WCF).

## Setup

MonolithicExtensions shouldn't have any external project/file references (they
should all be included in the solution), however if you want all the unit tests
to pass, you will need to install the TestService contained in this project.
Simply run the "installService.bat" script in the 
MonolithicExtensions.TestService/bin/debug directory to install the service
(assuming it still works) and you should be good. It doesn't do anything fancy,
so if the script doesn't work, you can install the service manually.

Many of the unit tests require various files stored in the unit test project
directory. These files may seem like garbage (and may be ignored in some .gitignore
configurations), but they are definitely needed for the unit tests to pass.
