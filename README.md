# test_20200305_p2p

Some considerations about this project :

Why C# ?
- socket api is, imho, one of the worst designed api, too error prone, too many corner cases, etc. so doing this project in a managed environment remove some caveats so it is at least bearable.

Why the UI ?
- having a ui instead of a command line program allows me to separate input entering from output screen printing, ie. in a command line program which does several things in parallel (like here, receiving messages while writing one) input stream and output stream get tangled and become unreadable. being able to have one ui component to write a message while reading received messages in another ui component is more user friendly ;
- having a ui allows me to have an already written message pump, no need to handle one by myself, so i can write directly an application which works on message sending without having to write the mechanism myself.

Caveats of these chosen technologies :
- windows platform only, might be less of a problem if i remove the ui and change from .net framework to core ;
- visual studio project, less easy to automate in a build script, but should be "open and build without worries" unlike most makefile projects where dependencies are always missing ;
- time consuming, by doing an ui, a lot of time is spent doing this ui, solving problems that are not required by the exercise.

Questions :

What are the limitations of this solution? Are these cases where yout service will not work?
Does your system scale? Where is the bottleneck? How many users can it support?
What is the attack surface on the system? How could you reduce it?
Compatibility: which OS/browsers/systems is our service compatible with?
