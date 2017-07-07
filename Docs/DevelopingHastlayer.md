# Developing Hastlayer



## Design principles

- From a user's (i.e. using developer's) perspective Hastlayer should be as simple as possible. To achieve this e.g. use generally good default configurations so in the majority of cases there is no configuration needed.
- Software that was written previously, without knowing about Hastlayer should be usable if it can live within the constraints of transformable code. E.g. users should never be forced to use custom attributes or other Hastlayer-specific elements in their code if the same effect can be achieved with runtime configuration (think about how members to be processed are configured: when running Hastlayer, not with attributes).
- If some code uses unsupported constructs it should be made apparent with exceptions. The hardware implementation silently failing (or working unexpectedly) should be avoided. Exceptions should include contextual information (e.g. the full expression or method of the problematic source) and offer hints on possible solutions.


## Flavors of the Hastlayer solution

The Hastlayer solution comes in two "flavors" with corresponding branches:

- Developer (*dev* branch): This is used by developers of Hastlayer itself. It includes the full source code.
- Client (*client* branch): Used by end-users of Hastlayer who run Hastlayer in a client mode, accessing *Hast.Core* as a remote service.

To allow the same code in the samples and elsewhere to support both scenarios Orchard's dynamic module loading needs to be utilized. For this to work *Hast.Core* projects should adhere to the following:

- Their projects need to be located in a subfolder of *Hast.Core*.
- They should include a *Module.txt* file (can be empty or can contain the usual Orchard configuration like `Dependencies`).
- Both the Debug and Release build output directories should be set just to *bin\\*.

If a Hast.Core projects needs to have types accessible in the Client flavor then create a corresponding `Abstractions` project. Such projects should follow the same rules listed above as *Hast.Core* projects with the only difference being that they should be located in a subfolder of *Hast.Abstractions*.

Note that if either kinds of projects reference another project that should be treated in the same way, with a manifest file and build output directories set (see e.g. `Hast.VhdlBuilder`).