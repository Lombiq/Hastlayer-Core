# Developing Hastlayer



## Design principles

- From a user's (i.e. using developer's) perspective Hastlayer should be as simple as possible. To achieve this e.g. use generally good default configurations so in the majority of cases there is no configuration needed.
- Software that was written previously, without knowing about Hastlayer should be usable if it can live within the constraints of transformable code. E.g. users should never be forced to use custom attributes or other Hastlayer-specific elements in their code if the same effect can be achieved with runtime configuration (think about how members to be processed are configured: when running Hastlayer, not with attributes).
- If some code uses unsupported constructs it should be made apparent with exceptions. The hardware implementation silently failing (or working unexpectedly) should be avoided.