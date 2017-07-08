# Getting started



These would be your first steps on starting to work with Hastlayer:

1. Clone the [Hastlayer repo](https://bitbucket.org/Lombiq/hastlayer) and the [Hastlayer Hardware](https://bitbucket.org/Lombiq/hastlayer-hardware) repo from Bitbucket (these are Mercurial repositories, similar to Git, that you can interact with via a GUI with e.g. [TortoiseHg](https://tortoisehg.bitbucket.io/); if you're unfamiliar with TortoiseHg check out [our video tutorial](https://www.youtube.com/watch?v=sbRxMXVEDc0)).
2. Update both repos to the `client` branch if you're a Hastlayer user, to `dev` if you're developing Hastlayer itself.
3. Set up a Vivado and Xilinx SDK project in the Hardware project as documented there, power up and program a compatible FPGA board.
4. Set the `Hast.Samples.Consumer` project as the  startup project here and start it. That will by default run the sample that is also added by default to the Hardware project.
5. You should be able to see the results of the sample in its console window.

If everything is alright follow up with the rest of this documentation.