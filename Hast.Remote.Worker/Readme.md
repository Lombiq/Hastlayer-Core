# Hastlayer - Remote Worker readme



This project contains the worker implementation that is invoked when clients use remote Hastlayer services. This is necessary in the Client flavor of Hastlayer.



## Important developer notes

All otherwise dynamically loaded extensions like *Hast.Transformer* need to be statically loaded as imported extensions, see `TransformationWorker`. This is needed because when used in the Daemon this library needs to be xcopy-deployable.