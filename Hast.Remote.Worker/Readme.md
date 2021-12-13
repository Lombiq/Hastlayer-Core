# Hastlayer - Remote Worker



This project contains the worker implementation that is invoked when clients use remote Hastlayer services. This is necessary in the Client flavor of Hastlayer.


## Important developer notes

All otherwise dynamically loaded extensions like *Hast.Transformer* need to be statically loaded as imported extensions, see `TransformationWorker`. This is needed because when used in the Daemon this library needs to be xcopy-deployable.


## Using Remote Worker Locally

Follow these steps to run _hastlayer.com_ on your local server:

1. Set up _Lombiq-Tenants_ as described [here](https://lombiq.atlassian.net/wiki/spaces/NEST/pages/3964950/Developer+overview#Local-development).
   - Note that you need to [set up IIS](https://docs.orchardproject.net/en/latest/Documentation/Manually-installing-Orchard-zip-file/) as well.
   - Add `127.0.0.1 hastlayer.com.localhost` to your hosts file (located in _%WINDIR%\system32\drivers\etc_ on Windows) otherwise RestEase won't be able to reach the local domain.
2. Go to http://hastlayer.com.localhost/Hastlayer.Frontend/AppManagement/ and click the _Add New Hastlayer Application_ button.
3. Enter `Hast.Samples.Consumer`, save and copy the resulting password. 
4. Configure _Hast.Remote.Worker.Daemon_ as described in their respective _Readme.md_ files.
5. [Start up Azurite](https://github.com/Azure/Azurite#getting-started).
6. Make sure the _transformation_ blob in your Azurite Blob Storage is empty. (If you are using Docker you can quickly delete the container and create a new one.)
7. Start up _Hast.Remote.Worker.Daemon_. (Just for testing, you can launch the executable directly.)
8. Start up _Hast.Samples.Consumer_ with the `-endpoint "http://hastlayer.com.localhost/api/Hastlayer.Frontend/" -appname "Hast.Samples.Consumer" -appsecret "THE PASSWORD"`
