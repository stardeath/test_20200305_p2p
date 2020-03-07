# test_20200305_p2p

Some considerations about this project :

Why C# ?
- socket api is, imho, one of the worst designed api, cumbersome, too many corner cases, etc. so doing this project in a managed environment remove some caveats so it is at least bearable.

Why the UI ?
- having a ui instead of a command line program allows me to separate input entering from output screen printing, ie. in a command line program which does several things in parallel (like here, receiving messages while writing one) input stream and output stream get tangled and become unreadable. being able to have one ui component to write a message while reading received messages in another ui component is more user friendly ;
- having a ui allows me to have an already written message pump, no need to handle one by myself, so i can write directly an application which works on message sending without having to write the mechanism myself.

Why UDP socket ?
- much less configuration than TCP
- receiving one datagram each time, and most of the time : one datagram == one high level message.

Why no high level messaging api (or other api) ?
- should work on a visual studio vanilla

Caveats of these chosen technologies :
- windows platform only, might be less of a problem if i remove the ui and change from .net framework to core ;
- visual studio project, less easy to automate in a build script, but should be "open and build without worries" unlike most makefile projects where dependencies are always missing ;
- time consuming, by doing an ui, a lot of time is spent doing this ui, solving problems that are not required by the exercise ;
- even if using udp is simpler than tcp, without messaging api, you should design a message protocol so multiple clients/servers can communicate ; hence you need at least to create coding/decoding message.

Questions :

What are the limitations of this solution? Are these cases where your service will not work?

- not tested on a real network, just local tests, so this exercise is prone to network defects, firewall, etc.

Does your system scale? Where is the bottleneck? How many users can it support?

- you need at least one "central" server to get the first peers, after that, since this system asks everybody, it is less affected if some portion of the network fails.

What is the attack surface on the system? How could you reduce it?

- every step of this system is at risk, to reduce it, it should have at least :
    - secured authentication
    - message encryption
    - user identification with a token, not it's name only

Compatibility: which OS/browsers/systems is our service compatible with?

- tested on windows 10 with a installed visual studio 2019
